using ControleFacil.Domain.Entities;

namespace ControleFacil.Domain.Interfaces;

public interface IInvestmentEntryRepository : IRepository<InvestmentEntry>
{
    IQueryable<InvestmentEntry> QueryWithDetails();
}
