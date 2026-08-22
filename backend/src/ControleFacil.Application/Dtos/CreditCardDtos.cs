namespace ControleFacil.Application.Dtos;

public record CreditCardCreateDto(string Name, int ClosingDay, int DueDay);

public record CreditCardUpdateDto(string Name, int ClosingDay, int DueDay, bool IsActive);

public record CreditCardResponseDto(int Id, string Name, int ClosingDay, int DueDay, bool IsActive);

public record CreditCardInvoiceDto(
    int CreditCardId,
    string CreditCardName,
    int Year,
    int Month,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly DueDate,
    decimal Total,
    IReadOnlyList<TransactionResponseDto> Transactions);
