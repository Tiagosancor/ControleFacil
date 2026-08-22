using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class ReportServiceTests
{
    private static async Task<(Category incomeGroup, Category expenseGroup, BankAccount account)> SeedAsync(
        ControleFacil.Domain.Interfaces.IUnitOfWork uow)
    {
        var incomeGroup = new Category { Name = "RENDA", Type = CategoryType.Income, UserId = 1, IsActive = true };
        var expenseGroup = new Category { Name = "DESPESA", Type = CategoryType.Expense, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(incomeGroup);
        await uow.Categories.AddAsync(expenseGroup);
        var account = new BankAccount { Name = "Banco", InitialBalance = 0, UserId = 1, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();
        return (incomeGroup, expenseGroup, account);
    }

    private static Transaction MakeTransaction(DateOnly entryDate, Category category, BankAccount account, decimal amount, TransactionStatus status = TransactionStatus.Paid) => new()
    {
        EntryDate = entryDate,
        CategoryId = category.Id,
        Description = $"Lançamento {entryDate}",
        PaymentMethod = PaymentMethod.Cash,
        BankAccountId = account.Id,
        Amount = amount,
        Status = status,
        UserId = 1,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetDreAsync_AggregatesByMonthAndGroup_AcrossTheYear()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new ReportService(uow, new FakeCurrentUserService(1));
        var (incomeGroup, expenseGroup, account) = await SeedAsync(uow);

        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 1, 5), incomeGroup, account, 5000m));
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 2, 5), incomeGroup, account, 5200m));
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 1, 10), expenseGroup, account, 1500m));
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2025, 12, 20), incomeGroup, account, 9999m)); // fora do ano, ignorado
        await uow.SaveChangesAsync();

        var report = await service.GetDreAsync(2026);

        var incomeRow = Assert.Single(report.IncomeRows);
        Assert.Equal(5000m, incomeRow.MonthlyValues[0]);
        Assert.Equal(5200m, incomeRow.MonthlyValues[1]);
        Assert.Equal(10200m, incomeRow.Total);

        var expenseRow = Assert.Single(report.ExpenseRows);
        Assert.Equal(1500m, expenseRow.MonthlyValues[0]);

        Assert.Equal(5000m, report.MonthlyIncomeTotals[0]);
        Assert.Equal(1500m, report.MonthlyExpenseTotals[0]);
        Assert.Equal(3500m, report.MonthlyBalance[0]);
        Assert.Equal(10200m, report.YearIncomeTotal);
        Assert.Equal(1500m, report.YearExpenseTotal);
        Assert.Equal(8700m, report.YearBalance);
    }

    [Fact]
    public async Task GetDreAsync_GroupsSubcategoriesUnderRootCategory()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new ReportService(uow, new FakeCurrentUserService(1));
        var (_, expenseGroup, account) = await SeedAsync(uow);
        var subcategory = new Category { Name = "Aluguel", Type = CategoryType.Expense, ParentCategoryId = expenseGroup.Id, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(subcategory);
        await uow.SaveChangesAsync();

        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 5), subcategory, account, 800m));
        await uow.SaveChangesAsync();

        var report = await service.GetDreAsync(2026);

        var row = Assert.Single(report.ExpenseRows);
        Assert.Equal(expenseGroup.Id, row.CategoryGroupId);
        Assert.Equal("DESPESA", row.CategoryGroupName);
        Assert.Equal(800m, row.MonthlyValues[2]);
    }

    [Fact]
    public async Task GetPendingAsync_SplitsPayableAndReceivable_AndFlagsOverdue()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new ReportService(uow, new FakeCurrentUserService(1));
        var (incomeGroup, expenseGroup, account) = await SeedAsync(uow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await uow.Transactions.AddAsync(MakeTransaction(today.AddDays(-5), expenseGroup, account, 300m, TransactionStatus.Pending)); // vencido
        await uow.Transactions.AddAsync(MakeTransaction(today.AddDays(10), expenseGroup, account, 150m, TransactionStatus.Pending)); // a vencer
        await uow.Transactions.AddAsync(MakeTransaction(today.AddDays(3), incomeGroup, account, 2000m, TransactionStatus.Pending)); // a receber
        await uow.Transactions.AddAsync(MakeTransaction(today, incomeGroup, account, 500m, TransactionStatus.Paid)); // pago, deve ficar de fora
        await uow.SaveChangesAsync();

        var report = await service.GetPendingAsync();

        Assert.Equal(2, report.Payable.Count);
        Assert.Single(report.Receivable);
        Assert.Equal(450m, report.TotalPayable);
        Assert.Equal(2000m, report.TotalReceivable);

        var overdue = Assert.Single(report.Payable, p => p.Overdue);
        Assert.Equal(300m, overdue.Amount);
    }

    [Fact]
    public async Task GetPendingAsync_ScopedByUser()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var (incomeGroup, _, account) = await SeedAsync(uow);
        await uow.Transactions.AddAsync(MakeTransaction(DateOnly.FromDateTime(DateTime.UtcNow), incomeGroup, account, 100m, TransactionStatus.Pending));
        await uow.SaveChangesAsync();

        var serviceUser2 = new ReportService(uow, new FakeCurrentUserService(2));
        var report = await serviceUser2.GetPendingAsync();

        Assert.Empty(report.Payable);
        Assert.Empty(report.Receivable);
    }
}
