using ControleFacil.Domain.Entities;

namespace ControleFacil.Domain.Interfaces;

public interface ILongTermGoalRepository : IRepository<LongTermGoal>
{
    IQueryable<LongTermGoal> QueryWithDetails();
}
