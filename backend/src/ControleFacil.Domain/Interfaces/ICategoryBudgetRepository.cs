using ControleFacil.Domain.Entities;

namespace ControleFacil.Domain.Interfaces;

public interface ICategoryBudgetRepository : IRepository<CategoryBudget>
{
    IQueryable<CategoryBudget> QueryWithDetails();
}
