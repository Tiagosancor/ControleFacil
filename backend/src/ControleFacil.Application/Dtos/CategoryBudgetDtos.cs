namespace ControleFacil.Application.Dtos;

public record CategoryBudgetCreateDto(int CategoryId, int Year, int Month, decimal LimitAmount);

public record CategoryBudgetUpdateDto(int CategoryId, int Year, int Month, decimal LimitAmount);

public record CategoryBudgetResponseDto(
    int Id,
    int CategoryId,
    string CategoryName,
    int Year,
    int Month,
    decimal LimitAmount,
    decimal Spent,
    decimal Percentage);
