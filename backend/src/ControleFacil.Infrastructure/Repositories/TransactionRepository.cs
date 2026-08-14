using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<Transaction> QueryWithDetails() => Set.Include(t => t.Category).Include(t => t.BankAccount);
}
