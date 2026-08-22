using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ReportService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    // DRE considera TODOS os lançamentos do ano (pagos ou não), mesma semântica do
    // "Resumo do mês" do Dashboard — é uma visão de planejamento por competência
    // (EntryDate), não um extrato de caixa.
    public async Task<DreReportDto> GetDreAsync(int year)
    {
        var rows = await _unitOfWork.Transactions.Query()
            .Where(t => t.UserId == _currentUser.UserId && t.EntryDate.Year == year)
            .Select(t => new
            {
                t.Amount,
                Month = t.EntryDate.Month,
                CategoryType = t.Category!.Type,
                GroupId = t.Category!.ParentCategoryId ?? t.Category!.Id,
                GroupName = t.Category!.ParentCategoryId == null ? t.Category!.Name : t.Category!.ParentCategory!.Name,
            })
            .ToListAsync();

        var incomeRows = BuildRows(rows.Where(r => r.CategoryType == CategoryType.Income)
            .Select(r => (r.GroupId, r.GroupName, r.Month, r.Amount)));
        var expenseRows = BuildRows(rows.Where(r => r.CategoryType == CategoryType.Expense)
            .Select(r => (r.GroupId, r.GroupName, r.Month, r.Amount)));

        var monthlyIncomeTotals = SumPerMonth(incomeRows);
        var monthlyExpenseTotals = SumPerMonth(expenseRows);
        var monthlyBalance = monthlyIncomeTotals.Zip(monthlyExpenseTotals, (income, expense) => income - expense).ToList();

        return new DreReportDto(
            year,
            incomeRows,
            expenseRows,
            monthlyIncomeTotals,
            monthlyExpenseTotals,
            monthlyBalance,
            monthlyIncomeTotals.Sum(),
            monthlyExpenseTotals.Sum(),
            monthlyIncomeTotals.Sum() - monthlyExpenseTotals.Sum());
    }

    private static List<DreRowDto> BuildRows(IEnumerable<(int GroupId, string GroupName, int Month, decimal Amount)> items)
    {
        return items
            .GroupBy(r => new { r.GroupId, r.GroupName })
            .Select(g =>
            {
                var monthlyValues = new decimal[12];
                foreach (var item in g)
                    monthlyValues[item.Month - 1] += item.Amount;

                return new DreRowDto(g.Key.GroupId, g.Key.GroupName, monthlyValues, monthlyValues.Sum());
            })
            .OrderByDescending(r => r.Total)
            .ToList();
    }

    private static List<decimal> SumPerMonth(IReadOnlyList<DreRowDto> rows)
    {
        var totals = new decimal[12];
        foreach (var row in rows)
            for (var i = 0; i < 12; i++)
                totals[i] += row.MonthlyValues[i];

        return totals.ToList();
    }

    // "Vencimento" = EntryDate (mesma convenção do alerta de vencimento da Sprint F —
    // não há campo de data separado no schema). Sem recorte de mês: mostra TODOS os
    // lançamentos pendentes, passados ou futuros.
    public async Task<PendingReportDto> GetPendingAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var items = await _unitOfWork.Transactions.Query()
            .Where(t => t.UserId == _currentUser.UserId && t.Status == TransactionStatus.Pending)
            .Select(t => new
            {
                t.Id,
                t.Description,
                t.Amount,
                t.EntryDate,
                CategoryName = t.Category!.Name,
                CategoryType = t.Category!.Type,
            })
            .OrderBy(t => t.EntryDate)
            .ToListAsync();

        var dtos = items
            .Select(t => new PendingItemDto(t.Id, t.Description, t.Amount, t.EntryDate, t.EntryDate < today, t.CategoryName, t.CategoryType))
            .ToList();

        var payable = dtos.Where(t => t.CategoryType == CategoryType.Expense).ToList();
        var receivable = dtos.Where(t => t.CategoryType == CategoryType.Income).ToList();

        return new PendingReportDto(payable, receivable, payable.Sum(t => t.Amount), receivable.Sum(t => t.Amount));
    }
}
