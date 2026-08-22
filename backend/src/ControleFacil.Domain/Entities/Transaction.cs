using ControleFacil.Domain.Enums;

namespace ControleFacil.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public DateOnly EntryDate { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Description { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; }
    public int BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    // Vínculo opcional com o cartão de crédito da compra (Sprint M) — usado só pra
    // agrupar o lançamento na fatura correta. Não muda o comportamento existente de
    // Saldo Total/Resumo do mês, que continua olhando só BankAccountId e Status.
    public int? CreditCardId { get; set; }
    public CreditCard? CreditCard { get; set; }

    public decimal Amount { get; set; }
    public DateOnly? PaymentDate { get; set; }
    public TransactionStatus Status { get; set; }
    public int? InstallmentNumber { get; set; }
    public int? TotalInstallments { get; set; }
    public int? SeriesId { get; set; }
    public TransactionSeries? Series { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Marca que o alerta de vencimento (Sprint F) já foi enviado por e-mail pra esse
    // lançamento — garante no-máximo-um envio mesmo com o job rodando de forma
    // irregular (o processo reinicia a cada deploy/sono do Render, então não dá pra
    // confiar só em estado em memória). Null = ainda não avisado.
    public DateTime? DueAlertSentAt { get; set; }
}
