using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class CreditCardServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidCard_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CreditCardService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new CreditCardCreateDto("Nubank", 10, 17));

        Assert.Equal("Nubank", result.Name);
        Assert.Equal(10, result.ClosingDay);
        Assert.Equal(17, result.DueDay);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes_SetsIsActiveFalse()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CreditCardService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new CreditCardCreateDto("Inter", 5, 12));

        await service.DeleteAsync(created.Id);
        var reloaded = await service.GetByIdAsync(created.Id);

        Assert.False(reloaded.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_AnotherUsersCard_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUser1 = new CreditCardService(uow, new FakeCurrentUserService(1));
        var created = await serviceUser1.CreateAsync(new CreditCardCreateDto("Nubank", 10, 17));

        var serviceUser2 = new CreditCardService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUser2.GetByIdAsync(created.Id));
    }

    private static async Task<(int categoryId, int accountId)> SeedCategoryAndAccountAsync(ControleFacil.Domain.Interfaces.IUnitOfWork uow)
    {
        var category = new Category { Name = "Compras", Type = CategoryType.Expense, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(category);
        var account = new BankAccount { Name = "Banco", InitialBalance = 0, UserId = 1, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();
        return (category.Id, account.Id);
    }

    [Fact]
    public async Task GetInvoiceAsync_DueDayAfterClosingDay_SameMonth_ComputesPeriodAndTotal()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var currentUser = new FakeCurrentUserService(1);
        var cardService = new CreditCardService(uow, currentUser);
        var (categoryId, accountId) = await SeedCategoryAndAccountAsync(uow);

        // Fecha dia 10, vence dia 17 (mesmo mês)
        var card = await cardService.CreateAsync(new CreditCardCreateDto("Nubank", 10, 17));
        var now = DateTime.UtcNow;

        // Dentro do período de março (11/fev a 10/mar)
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 5),
            CategoryId = categoryId,
            Description = "Compra dentro",
            PaymentMethod = PaymentMethod.Credit,
            BankAccountId = accountId,
            CreditCardId = card.Id,
            Amount = 100m,
            Status = TransactionStatus.Pending,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        // Fora do período (depois do fechamento de março, cai na fatura de abril)
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 15),
            CategoryId = categoryId,
            Description = "Compra fora",
            PaymentMethod = PaymentMethod.Credit,
            BankAccountId = accountId,
            CreditCardId = card.Id,
            Amount = 999m,
            Status = TransactionStatus.Pending,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var invoice = await cardService.GetInvoiceAsync(card.Id, 2026, 3);

        Assert.Equal(new DateOnly(2026, 2, 11), invoice.PeriodStart);
        Assert.Equal(new DateOnly(2026, 3, 10), invoice.PeriodEnd);
        Assert.Equal(new DateOnly(2026, 3, 17), invoice.DueDate);
        Assert.Equal(100m, invoice.Total);
        Assert.Single(invoice.Transactions);
    }

    [Fact]
    public async Task GetInvoiceAsync_DueDayBeforeClosingDay_RollsToNextMonth()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var cardService = new CreditCardService(uow, new FakeCurrentUserService(1));

        // Fecha dia 28, vence dia 5 do mês seguinte
        var card = await cardService.CreateAsync(new CreditCardCreateDto("Inter", 28, 5));

        var invoice = await cardService.GetInvoiceAsync(card.Id, 2026, 3);

        Assert.Equal(new DateOnly(2026, 4, 5), invoice.DueDate);
    }

    [Fact]
    public async Task GetInvoiceAsync_ClosingDayBeyondMonthLength_ClampsToLastDay()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var cardService = new CreditCardService(uow, new FakeCurrentUserService(1));

        // Fecha dia 31 — fevereiro só tem 28 dias em 2026 (não bissexto)
        var card = await cardService.CreateAsync(new CreditCardCreateDto("Cartão", 31, 5));

        var invoice = await cardService.GetInvoiceAsync(card.Id, 2026, 2);

        Assert.Equal(new DateOnly(2026, 2, 28), invoice.PeriodEnd);
    }

    [Fact]
    public async Task GetInvoiceAsync_CardFromAnotherUser_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var cardServiceUser1 = new CreditCardService(uow, new FakeCurrentUserService(1));
        var card = await cardServiceUser1.CreateAsync(new CreditCardCreateDto("Nubank", 10, 17));

        var cardServiceUser2 = new CreditCardService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() => cardServiceUser2.GetInvoiceAsync(card.Id, 2026, 3));
    }
}
