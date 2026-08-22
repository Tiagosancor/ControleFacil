namespace ControleFacil.Application.Dtos;

public record LongTermGoalCreateDto(
    string Name,
    decimal TargetAmount,
    int TargetYear,
    int TargetMonth,
    int? InvestmentCategoryId,
    decimal ManualCurrentAmount);

public record LongTermGoalUpdateDto(
    string Name,
    decimal TargetAmount,
    int TargetYear,
    int TargetMonth,
    int? InvestmentCategoryId,
    decimal ManualCurrentAmount);

public record LongTermGoalResponseDto(
    int Id,
    string Name,
    decimal TargetAmount,
    int TargetYear,
    int TargetMonth,
    int? InvestmentCategoryId,
    string? InvestmentCategoryName,
    decimal CurrentAmount,
    decimal ProgressPercentage,
    int MonthsRemaining,
    decimal MonthlyContributionNeeded);
