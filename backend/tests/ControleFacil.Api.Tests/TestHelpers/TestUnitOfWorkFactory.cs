using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;
using ControleFacil.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Api.Tests.TestHelpers;

public static class TestUnitOfWorkFactory
{
    public static IUnitOfWork Create(out AppDbContext context)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new AppDbContext(options);

        return new UnitOfWork(
            context,
            new UserRepository(context),
            new CategoryRepository(context),
            new BankAccountRepository(context),
            new TransactionRepository(context),
            new TransactionSeriesRepository(context),
            new PasswordResetTokenRepository(context),
            new CategoryBudgetRepository(context));
    }
}
