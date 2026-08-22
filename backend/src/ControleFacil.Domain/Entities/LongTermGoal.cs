namespace ControleFacil.Domain.Entities;

public class LongTermGoal
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public int TargetYear { get; set; }
    public int TargetMonth { get; set; }

    // Usado como "valor atual" apenas quando InvestmentCategoryId é null — quando a meta
    // está vinculada a uma categoria de investimento, o valor atual vem do último
    // InvestmentEntry daquela categoria em vez de ser digitado aqui.
    public decimal ManualCurrentAmount { get; set; }

    public int? InvestmentCategoryId { get; set; }
    public InvestmentCategory? InvestmentCategory { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
