using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Repositories;

public class LongTermGoalRepository : Repository<LongTermGoal>, ILongTermGoalRepository
{
    public LongTermGoalRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<LongTermGoal> QueryWithDetails() => Set.Include(g => g.InvestmentCategory);
}
