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
}
