namespace ControleFacil.Domain.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IBankAccountRepository BankAccounts { get; }
    ITransactionRepository Transactions { get; }
    ITransactionSeriesRepository TransactionSeries { get; }
    IPasswordResetTokenRepository PasswordResetTokens { get; }
    ICategoryBudgetRepository CategoryBudgets { get; }
    IMonthlyGoalRepository MonthlyGoals { get; }
    IInvestmentCategoryRepository InvestmentCategories { get; }
    IInvestmentEntryRepository InvestmentEntries { get; }
    ILongTermGoalRepository LongTermGoals { get; }
    ICreditCardRepository CreditCards { get; }
    Task<int> SaveChangesAsync();
}
