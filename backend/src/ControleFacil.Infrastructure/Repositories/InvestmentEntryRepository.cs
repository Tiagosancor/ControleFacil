using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Repositories;

public class InvestmentEntryRepository : Repository<InvestmentEntry>, IInvestmentEntryRepository
{
    public InvestmentEntryRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<InvestmentEntry> QueryWithDetails() => Set.Include(e => e.InvestmentCategory);
}
