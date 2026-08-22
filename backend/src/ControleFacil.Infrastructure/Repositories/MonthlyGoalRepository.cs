using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;

namespace ControleFacil.Infrastructure.Repositories;

public class MonthlyGoalRepository : Repository<MonthlyGoal>, IMonthlyGoalRepository
{
    public MonthlyGoalRepository(AppDbContext context) : base(context)
    {
    }
}
