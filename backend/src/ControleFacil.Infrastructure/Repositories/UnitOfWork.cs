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
        IPasswordResetTokenRepository passwordResetTokens,
        ICategoryBudgetRepository categoryBudgets,
        IMonthlyGoalRepository monthlyGoals,
        IInvestmentCategoryRepository investmentCategories,
        IInvestmentEntryRepository investmentEntries,
        ILongTermGoalRepository longTermGoals,
        ICreditCardRepository creditCards,
        IUsageEventRepository usageEvents,
        IBankRepository banks)
    {
        _context = context;
        Users = users;
        Categories = categories;
        BankAccounts = bankAccounts;
        Transactions = transactions;
        TransactionSeries = transactionSeries;
        PasswordResetTokens = passwordResetTokens;
        CategoryBudgets = categoryBudgets;
        MonthlyGoals = monthlyGoals;
        InvestmentCategories = investmentCategories;
        InvestmentEntries = investmentEntries;
        LongTermGoals = longTermGoals;
        CreditCards = creditCards;
        UsageEvents = usageEvents;
        Banks = banks;
    }

    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public IBankAccountRepository BankAccounts { get; }
    public ITransactionRepository Transactions { get; }
    public ITransactionSeriesRepository TransactionSeries { get; }
    public IPasswordResetTokenRepository PasswordResetTokens { get; }
    public ICategoryBudgetRepository CategoryBudgets { get; }
    public IMonthlyGoalRepository MonthlyGoals { get; }
    public IInvestmentCategoryRepository InvestmentCategories { get; }
    public IInvestmentEntryRepository InvestmentEntries { get; }
    public ILongTermGoalRepository LongTermGoals { get; }
    public ICreditCardRepository CreditCards { get; }
    public IUsageEventRepository UsageEvents { get; }
    public IBankRepository Banks { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
