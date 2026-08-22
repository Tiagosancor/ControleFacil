using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record TransactionCreateDto(
    DateOnly EntryDate,
    int CategoryId,
    string Description,
    PaymentMethod PaymentMethod,
    int BankAccountId,
    decimal Amount,
    DateOnly? PaymentDate,
    TransactionStatus Status,
    int? TotalInstallments,
    int? CreditCardId = null);

public record TransactionUpdateDto(
    DateOnly EntryDate,
    int CategoryId,
    string Description,
    PaymentMethod PaymentMethod,
    int BankAccountId,
    decimal Amount,
    DateOnly? PaymentDate,
    TransactionStatus Status,
    int? CreditCardId = null);

public record TransactionResponseDto(
    int Id,
    DateOnly EntryDate,
    int CategoryId,
    string CategoryName,
    string Description,
    PaymentMethod PaymentMethod,
    int BankAccountId,
    string BankAccountName,
    decimal Amount,
    DateOnly? PaymentDate,
    TransactionStatus Status,
    int? InstallmentNumber,
    int? TotalInstallments,
    int? SeriesId,
    int? CreditCardId = null,
    string? CreditCardName = null);

public record TransactionFilterDto(
    int? CategoryId,
    int? BankAccountId,
    TransactionStatus? Status,
    int? Year,
    int? Month,
    DateOnly? StartDate,
    DateOnly? EndDate);
