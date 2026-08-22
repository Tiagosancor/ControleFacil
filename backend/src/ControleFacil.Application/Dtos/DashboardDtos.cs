using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record CategoryBreakdownDto(int CategoryGroupId, string CategoryGroupName, CategoryType Type, decimal Total);

public record MonthlySummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    decimal TotalBalance,
    decimal TotalInvestments,
    IReadOnlyList<CategoryBreakdownDto> CategoryBreakdown);

// DeviationPercent é null quando a meta (denominador) é zero ou negativa — dividir por
// uma meta assim produziria um percentual sem leitura confiável (ex.: meta de lucro
// negativa por planejamento). O frontend mostra "—" nesse caso em vez de um número
// que pareceria errado.
public record GoalComparisonDto(decimal Realized, decimal Goal, decimal? DeviationPercent);

public record MonthlyGoalComparisonDto(GoalComparisonDto Income, GoalComparisonDto Expense, GoalComparisonDto Profit);

public record HistoricalMonthDto(int Year, int Month, decimal TotalIncome, decimal TotalExpense);

public record AnalysisDto(string Message, string Type);
