namespace ControleFacil.Application.Dtos;

public record DueAlertItemDto(
    int TransactionId,
    string Description,
    decimal Amount,
    DateOnly EntryDate,
    bool IsOverdue);
