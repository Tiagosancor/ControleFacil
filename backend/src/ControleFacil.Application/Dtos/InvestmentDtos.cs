using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record InvestmentCategoryCreateDto(
    string Name,
    InvestmentType Type,
    decimal AppliedAmount,
    decimal? InterestRate = null,
    decimal? MonthlyContribution = null);

public record InvestmentCategoryUpdateDto(
    string Name,
    InvestmentType Type,
    decimal AppliedAmount,
    bool IsActive,
    decimal? InterestRate = null,
    decimal? MonthlyContribution = null);

public record InvestmentCategoryResponseDto(
    int Id,
    string Name,
    InvestmentGroup? Group,
    InvestmentType? Type,
    decimal? AppliedAmount,
    decimal? InterestRate,
    decimal? MonthlyContribution,
    bool IsActive);

public record InvestmentEntryCreateDto(int InvestmentCategoryId, int Year, int Month, decimal Value);

public record InvestmentEntryUpdateDto(int InvestmentCategoryId, int Year, int Month, decimal Value);

public record InvestmentEntryResponseDto(
    int Id,
    int InvestmentCategoryId,
    string InvestmentCategoryName,
    int Year,
    int Month,
    decimal Value);
