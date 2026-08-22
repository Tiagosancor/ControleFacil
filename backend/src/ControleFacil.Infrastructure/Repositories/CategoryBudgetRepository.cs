using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Repositories;

public class CategoryBudgetRepository : Repository<CategoryBudget>, ICategoryBudgetRepository
{
    public CategoryBudgetRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<CategoryBudget> QueryWithDetails() => Set.Include(b => b.Category);
}
