using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    // Resumo mensal considera TODOS os lançamentos do mês (pagos ou não), refletindo o
    // planejamento do mês inteiro — diferente do saldo da conta bancária, que só conta
    // lançamentos pagos (reflete o extrato real).
    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month)
    {
        if (month is < 1 or > 12)
            throw new BusinessRuleException("Mês deve estar entre 1 e 12.");

        var rows = await _unitOfWork.Transactions.Query()
            .Where(t => t.UserId == _currentUser.UserId && t.EntryDate.Year == year && t.EntryDate.Month == month)
            .Select(t => new
            {
                t.Amount,
                CategoryType = t.Category!.Type,
                GroupId = t.Category!.ParentCategoryId ?? t.Category!.Id,
                GroupName = t.Category!.ParentCategoryId == null ? t.Category!.Name : t.Category!.ParentCategory!.Name,
            })
            .ToListAsync();

        var totalIncome = rows.Where(r => r.CategoryType == CategoryType.Income).Sum(r => r.Amount);
        var totalExpense = rows.Where(r => r.CategoryType == CategoryType.Expense).Sum(r => r.Amount);

        var breakdown = rows
            .GroupBy(r => new { r.GroupId, r.GroupName, r.CategoryType })
            .Select(g => new CategoryBreakdownDto(g.Key.GroupId, g.Key.GroupName, g.Key.CategoryType, g.Sum(r => r.Amount)))
            .OrderByDescending(b => b.Total)
            .ToList();

        var totalBalance = await GetTotalBalanceAsync();

        return new MonthlySummaryDto(year, month, totalIncome, totalExpense, totalIncome - totalExpense, totalBalance, breakdown);
    }

    // Saldo total = "quanto dinheiro o usuário tem agora": saldo inicial das contas bancárias
    // ATIVAS + todos os lançamentos já pagos dessas contas (receita soma, despesa subtrai),
    // sem recorte de mês — mesma semântica de saldo usada em BankAccountService, só que somada
    // entre todas as contas. Contas inativas (soft-deleted) e lançamentos pendentes ficam de fora.
    private async Task<decimal> GetTotalBalanceAsync()
    {
        var activeAccounts = await _unitOfWork.BankAccounts.Query()
            .Where(b => b.UserId == _currentUser.UserId && b.IsActive)
            .Select(b => new { b.Id, b.InitialBalance })
            .ToListAsync();

        var initialBalanceSum = activeAccounts.Sum(b => b.InitialBalance);
        var activeAccountIds = activeAccounts.Select(b => b.Id).ToList();
        if (activeAccountIds.Count == 0)
            return initialBalanceSum;

        var paidDelta = await _unitOfWork.Transactions.Query()
            .Where(t => activeAccountIds.Contains(t.BankAccountId) && t.UserId == _currentUser.UserId && t.Status == TransactionStatus.Paid)
            .Select(t => t.Category!.Type == CategoryType.Income ? t.Amount : -t.Amount)
            .SumAsync();

        return initialBalanceSum + paidDelta;
    }
}
