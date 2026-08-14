namespace ControleFacil.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IBankAccountRepository BankAccounts { get; }
    ITransactionRepository Transactions { get; }
    ITransactionSeriesRepository TransactionSeries { get; }
    Task<int> SaveChangesAsync();
}
