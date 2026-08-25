namespace ControleFacil.Domain.Entities;

public class BankAccount
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal InitialBalance { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public bool IsActive { get; set; } = true;

    // Opcional — Name continua sendo o apelido livre da conta (ex: "Caixinha", que não
    // é um banco de verdade), Bank é a instituição real escolhida na lista da BrasilAPI.
    public string? BankIspb { get; set; }
    public Bank? Bank { get; set; }
}
