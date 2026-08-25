using ControleFacil.Domain.Enums;

namespace ControleFacil.Domain.Entities;

public class InvestmentCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int UserId { get; set; }
    public User? User { get; set; }
    public bool IsActive { get; set; } = true;

    // Nulos representam categorias criadas antes desta classificação existir —
    // ainda não editadas pelo usuário. Create/Update exigem os dois preenchidos daqui
    // pra frente (validado em InvestmentCategoryCreateDto/UpdateDto), mas a coluna em
    // si fica nullable pra não quebrar linhas antigas na migration.
    public InvestmentType? Type { get; set; }
    public decimal? AppliedAmount { get; set; }
    public decimal? InterestRate { get; set; }
    public decimal? MonthlyContribution { get; set; }
}
