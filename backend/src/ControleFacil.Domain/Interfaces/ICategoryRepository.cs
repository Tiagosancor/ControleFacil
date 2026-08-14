using ControleFacil.Domain.Entities;

namespace ControleFacil.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    IQueryable<Category> QueryWithDetails();
}
