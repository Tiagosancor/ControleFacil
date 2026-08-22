using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class DueAlertServiceTests
{
    private static DueAlertService BuildService(
        ControleFacil.Domain.Interfaces.IUnitOfWork uow,
        FakeEmailService emailService,
        int daysBefore = 3)
    {
        return new DueAlertService(
            uow,
            emailService,
            Options.Create(new DueAlertOptions { DaysBefore = daysBefore }),
            NullLogger<DueAlertService>.Instance);
    }

    private static async Task<(User user, Category category, BankAccount account)> SeedUserAsync(
        ControleFacil.Domain.Interfaces.IUnitOfWork uow, string email = "teste@example.com")
    {
        var user = new User { Name = "Usuário Teste", Email = email, PasswordHash = "hash", CreatedAt = DateTime.UtcNow };
        await uow.Users.AddAsync(user);
        await uow.SaveChangesAsync();

        var category = new Category { Name = "Aluguel", Type = CategoryType.Expense, UserId = user.Id, IsActive = true };
        await uow.Categories.AddAsync(category);

        var account = new BankAccount { Name = "Banco 1", InitialBalance = 0, UserId = user.Id, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();

        return (user, category, account);
    }

    private static Transaction MakeTransaction(
        DateOnly entryDate, Category category, BankAccount account, User user,
        TransactionStatus status = TransactionStatus.Pending, DateTime? dueAlertSentAt = null,
        string description = "Aluguel") => new()
    {
        EntryDate = entryDate,
        CategoryId = category.Id,
        Description = description,
        PaymentMethod = PaymentMethod.Cash,
        BankAccountId = account.Id,
        Amount = 100,
        Status = status,
        DueAlertSentAt = dueAlertSentAt,
        UserId = user.Id,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task SendPendingDueAlertsAsync_SendsForPendingTransactionWithinWindow_AndMarksSent()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var emailService = new FakeEmailService();
        var service = BuildService(uow, emailService, daysBefore: 3);
        var (user, category, account) = await SeedUserAsync(uow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoon = MakeTransaction(today.AddDays(2), category, account, user);
        var farAway = MakeTransaction(today.AddDays(10), category, account, user, description: "Longe");
        var alreadyPaid = MakeTransaction(today.AddDays(1), category, account, user, status: TransactionStatus.Paid, description: "Pago");
        await uow.Transactions.AddAsync(dueSoon);
        await uow.Transactions.AddAsync(farAway);
        await uow.Transactions.AddAsync(alreadyPaid);
        await uow.SaveChangesAsync();

        var sent = await service.SendPendingDueAlertsAsync();

        Assert.Equal(1, sent);
        var call = Assert.Single(emailService.DueAlertCalls);
        Assert.Equal(user.Email, call.ToEmail);
        var item = Assert.Single(call.Items);
        Assert.Equal(dueSoon.Id, item.TransactionId);
        Assert.False(item.IsOverdue);

        Assert.NotNull(dueSoon.DueAlertSentAt);
        Assert.Null(farAway.DueAlertSentAt);
        Assert.Null(alreadyPaid.DueAlertSentAt);
    }

    [Fact]
    public async Task SendPendingDueAlertsAsync_MarksOverdueTransactions_AsIsOverdueTrue()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var emailService = new FakeEmailService();
        var service = BuildService(uow, emailService);
        var (user, category, account) = await SeedUserAsync(uow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdue = MakeTransaction(today.AddDays(-5), category, account, user);
        await uow.Transactions.AddAsync(overdue);
        await uow.SaveChangesAsync();

        await service.SendPendingDueAlertsAsync();

        var item = Assert.Single(Assert.Single(emailService.DueAlertCalls).Items);
        Assert.True(item.IsOverdue);
    }

    [Fact]
    public async Task SendPendingDueAlertsAsync_DoesNotResend_WhenAlreadyMarked()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var emailService = new FakeEmailService();
        var service = BuildService(uow, emailService);
        var (user, category, account) = await SeedUserAsync(uow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var alreadyAlerted = MakeTransaction(today.AddDays(1), category, account, user, dueAlertSentAt: DateTime.UtcNow.AddDays(-1));
        await uow.Transactions.AddAsync(alreadyAlerted);
        await uow.SaveChangesAsync();

        var sent = await service.SendPendingDueAlertsAsync();

        Assert.Equal(0, sent);
        Assert.Empty(emailService.DueAlertCalls);
    }

    [Fact]
    public async Task SendPendingDueAlertsAsync_GroupsMultipleDueTransactions_IntoOneEmailPerUser()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var emailService = new FakeEmailService();
        var service = BuildService(uow, emailService);
        var (user, category, account) = await SeedUserAsync(uow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await uow.Transactions.AddAsync(MakeTransaction(today.AddDays(1), category, account, user, description: "Conta 1"));
        await uow.Transactions.AddAsync(MakeTransaction(today.AddDays(2), category, account, user, description: "Conta 2"));
        await uow.SaveChangesAsync();

        var sent = await service.SendPendingDueAlertsAsync();

        Assert.Equal(1, sent);
        var call = Assert.Single(emailService.DueAlertCalls);
        Assert.Equal(2, call.Items.Count);
    }

    [Fact]
    public async Task SendPendingDueAlertsAsync_DoesNotMarkSent_WhenEmailFails()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var emailService = new FakeEmailService { ThrowOnDueAlert = true };
        var service = BuildService(uow, emailService);
        var (user, category, account) = await SeedUserAsync(uow);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoon = MakeTransaction(today.AddDays(1), category, account, user);
        await uow.Transactions.AddAsync(dueSoon);
        await uow.SaveChangesAsync();

        var sent = await service.SendPendingDueAlertsAsync();

        Assert.Equal(0, sent);
        Assert.Null(dueSoon.DueAlertSentAt);
    }
}
