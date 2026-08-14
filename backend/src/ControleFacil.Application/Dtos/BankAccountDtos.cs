namespace ControleFacil.Application.Dtos;

public record BankAccountCreateDto(string Name, decimal InitialBalance);

public record BankAccountUpdateDto(string Name, decimal InitialBalance, bool IsActive);

public record BankAccountResponseDto(int Id, string Name, decimal InitialBalance, bool IsActive);
