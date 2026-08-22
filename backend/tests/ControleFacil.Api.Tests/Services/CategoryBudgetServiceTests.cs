using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class CategoryBudgetServiceTests
{
    private static async Task<(Category group, Category sub, BankAccount account)> SeedExpenseGroupAsync(
        Domain.Interfaces.IUnitOfWork uow, int userId = 1, string groupName = "DESPESAS COM ALIMENTAÇÃO")
    {
        var group = new Category { Name = groupName, Type = CategoryType.Expense, UserId = userId, IsActive = true };
        await uow.Categories.AddAsync(group);
        await uow.SaveChangesAsync();

        var sub = new Category { Name = "Supermercado", Type = CategoryType.Expense, ParentCategoryId = group.Id, UserId = userId, IsActive = true };
        await uow.Categories.AddAsync(sub);

        var account = new BankAccount { Name = "Banco 1", InitialBalance = 0, UserId = userId, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();

        return (group, sub, account);
    }

    [Fact]
    public async Task CreateAsync_RootExpenseCategory_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var (group, _, _) = await SeedExpenseGroupAsync(uow);

        var result = await service.CreateAsync(new CategoryBudgetCreateDto(group.Id, 2026, 3, 800m));

        Assert.Equal(group.Id, result.CategoryId);
        Assert.Equal(800m, result.LimitAmount);
        Assert.Equal(0m, result.Spent);
    }

    [Fact]
    public async Task CreateAsync_SubCategory_ThrowsBusinessRuleException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var (_, sub, _) = await SeedExpenseGroupAsync(uow);

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(new CategoryBudgetCreateDto(sub.Id, 2026, 3, 800m)));
    }

    [Fact]
    public async Task CreateAsync_IncomeCategory_ThrowsBusinessRuleException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var incomeGroup = new Category { Name = "RENDA FAMILIAR", Type = CategoryType.Income, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(incomeGroup);
        await uow.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(new CategoryBudgetCreateDto(incomeGroup.Id, 2026, 3, 800m)));
    }

    [Fact]
    public async Task CreateAsync_DuplicateForSameCategoryAndMonth_ThrowsConflictException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var (group, _, _) = await SeedExpenseGroupAsync(uow);

        await service.CreateAsync(new CategoryBudgetCreateDto(group.Id, 2026, 3, 800m));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CategoryBudgetCreateDto(group.Id, 2026, 3, 500m)));
    }

    [Fact]
    public async Task GetAllAsync_ComputesSpent_FromRootAndSubCategoryTransactions()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var (group, sub, account) = await SeedExpenseGroupAsync(uow);

        await service.CreateAsync(new CategoryBudgetCreateDto(group.Id, 2026, 3, 800m));

        var now = DateTime.UtcNow;
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 5),
            CategoryId = sub.Id, // gasto numa subcategoria do grupo
            Description = "Compras",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 300m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 10),
            CategoryId = sub.Id,
            Description = "Feira",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 100m,
            Status = TransactionStatus.Pending, // pendente também conta
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 2, 5), // mês diferente: não conta
            CategoryId = sub.Id,
            Description = "Fevereiro",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 999m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var budgets = await service.GetAllAsync(2026, 3);

        var budget = Assert.Single(budgets);
        Assert.Equal(400m, budget.Spent);
        Assert.Equal(0.5m, budget.Percentage);
    }

    [Fact]
    public async Task UpdateAsync_ChangesFields_AndAllowsKeepingSameCategoryMonth()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var (group, _, _) = await SeedExpenseGroupAsync(uow);

        var created = await service.CreateAsync(new CategoryBudgetCreateDto(group.Id, 2026, 3, 800m));

        var updated = await service.UpdateAsync(created.Id, new CategoryBudgetUpdateDto(group.Id, 2026, 3, 1000m));

        Assert.Equal(1000m, updated.LimitAmount);
    }

    [Fact]
    public async Task DeleteAsync_RemovesBudget()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var (group, _, _) = await SeedExpenseGroupAsync(uow);

        var created = await service.CreateAsync(new CategoryBudgetCreateDto(group.Id, 2026, 3, 800m));
        await service.DeleteAsync(created.Id);

        var remaining = await service.GetAllAsync(2026, 3);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task GetByIdAsync_AnotherUsersBudget_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUser1 = new CategoryBudgetService(uow, new FakeCurrentUserService(1));
        var (group, _, _) = await SeedExpenseGroupAsync(uow, userId: 1);
        var created = await serviceUser1.CreateAsync(new CategoryBudgetCreateDto(group.Id, 2026, 3, 800m));

        var serviceUser2 = new CategoryBudgetService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUser2.GetByIdAsync(created.Id));
    }
}
