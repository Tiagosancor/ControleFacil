using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;

namespace ControleFacil.Infrastructure.Repositories;

public class InvestmentCategoryRepository : Repository<InvestmentCategory>, IInvestmentCategoryRepository
{
    public InvestmentCategoryRepository(AppDbContext context) : base(context)
    {
    }
}
