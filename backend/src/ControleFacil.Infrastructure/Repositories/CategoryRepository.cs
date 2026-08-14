using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<Category> QueryWithDetails() => Set.Include(c => c.ParentCategory);
}
