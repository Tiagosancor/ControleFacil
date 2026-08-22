namespace ControleFacil.Domain.Entities;

public class MonthlyGoal
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal IncomeGoal { get; set; }
    public decimal ExpenseGoal { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
