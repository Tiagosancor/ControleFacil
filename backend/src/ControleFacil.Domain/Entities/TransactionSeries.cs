namespace ControleFacil.Domain.Entities;

/// <summary>
/// Agrupa as parcelas/ocorrências de um mesmo lançamento parcelado ou recorrente,
/// permitindo editar ou cancelar a série inteira em vez de cada linha isoladamente.
/// </summary>
public class TransactionSeries
{
    public int Id { get; set; }
    public int TotalInstallments { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
