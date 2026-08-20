using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class TransactionServiceTests
{
    private static async Task<(Category category, BankAccount account)> SeedCategoryAndAccountAsync(
        Domain.Interfaces.IUnitOfWork uow)
    {
        var category = new Category { Name = "Salários", Type = CategoryType.Income, UserId = 1, IsActive = true };
        var account = new BankAccount { Name = "Banco 1", InitialBalance = 0, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(category);
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();
        return (category, account);
    }

    private static Transaction MakeTransaction(DateOnly entryDate, Category category, BankAccount account) => new()
    {
        EntryDate = entryDate,
        CategoryId = category.Id,
        Description = $"Lançamento {entryDate}",
        PaymentMethod = PaymentMethod.Cash,
        BankAccountId = account.Id,
        Amount = 100,
        Status = TransactionStatus.Paid,
        UserId = 1,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetAllAsync_StartAndEndDateEqual_FiltersSingleDay()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new TransactionService(uow, new FakeCurrentUserService(1));
        var (category, account) = await SeedCategoryAndAccountAsync(uow);

        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 14), category, account));
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 15), category, account));
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 16), category, account));
        await uow.SaveChangesAsync();

        var filter = new TransactionFilterDto(null, null, null, null, null, new DateOnly(2026, 3, 15), new DateOnly(2026, 3, 15));
        var result = await service.GetAllAsync(filter, page: 1, pageSize: 20);

        var item = Assert.Single(result.Items);
        Assert.Equal(new DateOnly(2026, 3, 15), item.EntryDate);
    }

    [Fact]
    public async Task GetAllAsync_DateRange_IsInclusiveOnBothEnds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new TransactionService(uow, new FakeCurrentUserService(1));
        var (category, account) = await SeedCategoryAndAccountAsync(uow);

        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 9), category, account)); // fora, antes
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 10), category, account)); // borda inicial
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 20), category, account)); // borda final
        await uow.Transactions.AddAsync(MakeTransaction(new DateOnly(2026, 3, 21), category, account)); // fora, depois
        await uow.SaveChangesAsync();

        var filter = new TransactionFilterDto(null, null, null, null, null, new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 20));
        var result = await service.GetAllAsync(filter, page: 1, pageSize: 20);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, t => t.EntryDate == new DateOnly(2026, 3, 10));
        Assert.Contains(result.Items, t => t.EntryDate == new DateOnly(2026, 3, 20));
    }
}
