using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;

namespace ControleFacil.Infrastructure.Repositories;

public class UsageEventRepository : Repository<UsageEvent>, IUsageEventRepository
{
    public UsageEventRepository(AppDbContext context) : base(context)
    {
    }
}
