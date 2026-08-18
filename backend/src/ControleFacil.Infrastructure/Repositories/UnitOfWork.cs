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
        ITransactionSeriesRepository transactionSeries,
        IPasswordResetTokenRepository passwordResetTokens)
    {
        _context = context;
        Users = users;
        Categories = categories;
        BankAccounts = bankAccounts;
        Transactions = transactions;
        TransactionSeries = transactionSeries;
        PasswordResetTokens = passwordResetTokens;
    }

    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public IBankAccountRepository BankAccounts { get; }
    public ITransactionRepository Transactions { get; }
    public ITransactionSeriesRepository TransactionSeries { get; }
    public IPasswordResetTokenRepository PasswordResetTokens { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
