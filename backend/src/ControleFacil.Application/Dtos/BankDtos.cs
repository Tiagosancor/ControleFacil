namespace ControleFacil.Application.Dtos;

public record BankDto(string Ispb, int? Code, string Name, string FullName, string? LogoUrl);

// Formato intermediário devolvido pela porta de infraestrutura (IBrasilApiBankClient) pro
// BankSyncService fazer o upsert — mantém o shape cru da BrasilAPI fora da Application.
public record BankSyncItemDto(string Ispb, int? Code, string Name, string FullName, string? LogoUrl);
