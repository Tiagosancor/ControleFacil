namespace ControleFacil.Domain.Entities;

public class InvestmentCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int UserId { get; set; }
    public User? User { get; set; }
    public bool IsActive { get; set; } = true;
}
