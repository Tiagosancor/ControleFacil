using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;

namespace ControleFacil.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IUserRepository users,
        ICategoryRepository categories,
        IBankAccountRepository bankAccounts,
        ITransactionRepository transactions,
        ITransactionSeriesRepository transactionSeries)
    {
        _context = context;
        Users = users;
        Categories = categories;
        BankAccounts = bankAccounts;
        Transactions = transactions;
        TransactionSeries = transactionSeries;
    }

    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public IBankAccountRepository BankAccounts { get; }
    public ITransactionRepository Transactions { get; }
    public ITransactionSeriesRepository TransactionSeries { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
