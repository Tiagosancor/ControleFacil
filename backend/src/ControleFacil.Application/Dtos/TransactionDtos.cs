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
    int? TotalInstallments);

public record TransactionUpdateDto(
    DateOnly EntryDate,
    int CategoryId,
    string Description,
    PaymentMethod PaymentMethod,
    int BankAccountId,
    decimal Amount,
    DateOnly? PaymentDate,
    TransactionStatus Status);

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
    int? SeriesId);

public record TransactionFilterDto(
    int? CategoryId,
    int? BankAccountId,
    TransactionStatus? Status,
    int? Year,
    int? Month);
