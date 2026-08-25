namespace ControleFacil.Application.Dtos;

public record BankAccountCreateDto(string Name, decimal InitialBalance, string? BankIspb = null);

public record BankAccountUpdateDto(string Name, decimal InitialBalance, bool IsActive, string? BankIspb = null);

public record BankAccountResponseDto(int Id, string Name, decimal InitialBalance, decimal CurrentBalance, bool IsActive, BankDto? Bank);
