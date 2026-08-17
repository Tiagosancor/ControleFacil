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

        return new MonthlySummaryDto(year, month, totalIncome, totalExpense, totalIncome - totalExpense, breakdown);
    }
}
