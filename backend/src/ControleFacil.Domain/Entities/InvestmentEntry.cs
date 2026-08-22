namespace ControleFacil.Domain.Entities;

public class InvestmentEntry
{
    public int Id { get; set; }
    public int InvestmentCategoryId { get; set; }
    public InvestmentCategory? InvestmentCategory { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Value { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
