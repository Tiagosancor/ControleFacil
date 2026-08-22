using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record DreRowDto(int CategoryGroupId, string CategoryGroupName, IReadOnlyList<decimal> MonthlyValues, decimal Total);

public record DreReportDto(
    int Year,
    IReadOnlyList<DreRowDto> IncomeRows,
    IReadOnlyList<DreRowDto> ExpenseRows,
    IReadOnlyList<decimal> MonthlyIncomeTotals,
    IReadOnlyList<decimal> MonthlyExpenseTotals,
    IReadOnlyList<decimal> MonthlyBalance,
    decimal YearIncomeTotal,
    decimal YearExpenseTotal,
    decimal YearBalance);

public record PendingItemDto(
    int TransactionId,
    string Description,
    decimal Amount,
    DateOnly DueDate,
    bool Overdue,
    string CategoryName,
    CategoryType CategoryType);

public record PendingReportDto(
    IReadOnlyList<PendingItemDto> Payable,
    IReadOnlyList<PendingItemDto> Receivable,
    decimal TotalPayable,
    decimal TotalReceivable);
