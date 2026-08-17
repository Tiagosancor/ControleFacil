using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record CategoryBreakdownDto(int CategoryGroupId, string CategoryGroupName, CategoryType Type, decimal Total);

public record MonthlySummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    IReadOnlyList<CategoryBreakdownDto> CategoryBreakdown);
