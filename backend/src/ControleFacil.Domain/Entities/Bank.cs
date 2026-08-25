namespace ControleFacil.Domain.Entities;

// Tabela global (sem UserId) sincronizada periodicamente a partir da BrasilAPI — ver
// BankSyncBackgroundService. Id sequencial como PK (padrão do projeto — IRepository<T>
// assume int), mas Ispb é a chave natural/estável do Banco Central: única, indexada, e é
// ela que BankAccount.BankIspb referencia (HasPrincipalKey), não o Id interno.
public class Bank
{
    public int Id { get; set; }
    public string Ispb { get; set; } = null!;
    public int? Code { get; set; }
    public string Name { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}
