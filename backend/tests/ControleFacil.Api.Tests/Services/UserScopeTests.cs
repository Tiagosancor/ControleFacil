using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class UserScopeTests
{
    [Fact]
    public async Task Category_OtherUser_CannotGetUpdateOrDelete()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUserA = new CategoryService(uow, new FakeCurrentUserService(1));
        var serviceUserB = new CategoryService(uow, new FakeCurrentUserService(2));

        var categoryFromA = await serviceUserA.CreateAsync(new CategoryCreateDto("Categoria da A", CategoryType.Expense, null));

        await Assert.ThrowsAsync<NotFoundException>(() => serviceUserB.GetByIdAsync(categoryFromA.Id));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            serviceUserB.UpdateAsync(categoryFromA.Id, new CategoryUpdateDto("Hackeada", CategoryType.Expense, null, true)));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUserB.DeleteAsync(categoryFromA.Id));

        var listFromB = await serviceUserB.GetAllAsync(includeInactive: true, page: 1, pageSize: 20);
        Assert.Empty(listFromB.Items);
    }

    [Fact]
    public async Task BankAccount_OtherUser_CannotGetUpdateOrDelete()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUserA = new BankAccountService(uow, new FakeCurrentUserService(1));
        var serviceUserB = new BankAccountService(uow, new FakeCurrentUserService(2));

        var accountFromA = await serviceUserA.CreateAsync(new BankAccountCreateDto("Conta da A", 1000m));

        await Assert.ThrowsAsync<NotFoundException>(() => serviceUserB.GetByIdAsync(accountFromA.Id));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            serviceUserB.UpdateAsync(accountFromA.Id, new BankAccountUpdateDto("Hackeada", 0m, true)));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUserB.DeleteAsync(accountFromA.Id));
    }

    [Fact]
    public async Task Transaction_CannotReferenceAnotherUsersCategoryOrBankAccount()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var categoryServiceUserA = new CategoryService(uow, new FakeCurrentUserService(1));
        var bankAccountServiceUserA = new BankAccountService(uow, new FakeCurrentUserService(1));
        var transactionServiceUserB = new TransactionService(uow, new FakeCurrentUserService(2));

        var categoryFromA = await categoryServiceUserA.CreateAsync(new CategoryCreateDto("Categoria da A", CategoryType.Expense, null));
        var accountFromA = await bankAccountServiceUserA.CreateAsync(new BankAccountCreateDto("Conta da A", 500m));

        var dto = new TransactionCreateDto(
            new DateOnly(2026, 1, 10),
            categoryFromA.Id,
            "Tentativa de invasão",
            PaymentMethod.Cash,
            accountFromA.Id,
            100m,
            null,
            TransactionStatus.Pending,
            null);

        await Assert.ThrowsAsync<NotFoundException>(() => transactionServiceUserB.CreateAsync(dto));
    }

    [Fact]
    public async Task Transaction_OtherUser_CannotGetUpdateOrDelete()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var categoryServiceUserA = new CategoryService(uow, new FakeCurrentUserService(1));
        var bankAccountServiceUserA = new BankAccountService(uow, new FakeCurrentUserService(1));
        var transactionServiceUserA = new TransactionService(uow, new FakeCurrentUserService(1));
        var transactionServiceUserB = new TransactionService(uow, new FakeCurrentUserService(2));

        var category = await categoryServiceUserA.CreateAsync(new CategoryCreateDto("Categoria da A", CategoryType.Expense, null));
        var account = await bankAccountServiceUserA.CreateAsync(new BankAccountCreateDto("Conta da A", 500m));

        var created = await transactionServiceUserA.CreateAsync(new TransactionCreateDto(
            new DateOnly(2026, 1, 10),
            category.Id,
            "Lançamento da A",
            PaymentMethod.Cash,
            account.Id,
            100m,
            null,
            TransactionStatus.Pending,
            null));
        var transactionId = created[0].Id;

        await Assert.ThrowsAsync<NotFoundException>(() => transactionServiceUserB.GetByIdAsync(transactionId));
        await Assert.ThrowsAsync<NotFoundException>(() => transactionServiceUserB.DeleteAsync(transactionId));
    }
}
