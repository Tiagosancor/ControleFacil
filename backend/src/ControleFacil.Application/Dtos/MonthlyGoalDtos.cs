namespace ControleFacil.Application.Dtos;

public record MonthlyGoalCreateDto(int Year, int Month, decimal IncomeGoal, decimal ExpenseGoal);

public record MonthlyGoalUpdateDto(int Year, int Month, decimal IncomeGoal, decimal ExpenseGoal);

public record MonthlyGoalResponseDto(int Id, int Year, int Month, decimal IncomeGoal, decimal ExpenseGoal);
