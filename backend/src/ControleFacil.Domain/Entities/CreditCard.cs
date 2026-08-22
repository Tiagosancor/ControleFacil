namespace ControleFacil.Domain.Entities;

public class CreditCard
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    // Dia do mês em que a fatura fecha — compras depois desse dia caem na fatura seguinte.
    public int ClosingDay { get; set; }

    // Dia do mês em que a fatura vence. Pode ser no mesmo mês do fechamento (ex: fecha
    // dia 10, vence dia 17) ou no mês seguinte (ex: fecha dia 28, vence dia 5) — o cálculo
    // do período/vencimento em CreditCardService trata os dois casos automaticamente.
    public int DueDay { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public bool IsActive { get; set; } = true;
}
