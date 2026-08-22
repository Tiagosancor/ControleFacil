namespace ControleFacil.Application.Dtos;

public record InvestmentCategoryCreateDto(string Name);

public record InvestmentCategoryUpdateDto(string Name, bool IsActive);

public record InvestmentCategoryResponseDto(int Id, string Name, bool IsActive);

public record InvestmentEntryCreateDto(int InvestmentCategoryId, int Year, int Month, decimal Value);

public record InvestmentEntryUpdateDto(int InvestmentCategoryId, int Year, int Month, decimal Value);

public record InvestmentEntryResponseDto(
    int Id,
    int InvestmentCategoryId,
    string InvestmentCategoryName,
    int Year,
    int Month,
    decimal Value);
