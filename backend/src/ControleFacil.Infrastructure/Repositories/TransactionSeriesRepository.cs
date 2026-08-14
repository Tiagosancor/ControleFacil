using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;

namespace ControleFacil.Infrastructure.Repositories;

public class TransactionSeriesRepository : Repository<TransactionSeries>, ITransactionSeriesRepository
{
    public TransactionSeriesRepository(AppDbContext context) : base(context)
    {
    }
}
