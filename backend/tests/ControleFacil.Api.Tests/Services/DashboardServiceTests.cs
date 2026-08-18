using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetMonthlySummaryAsync_AggregatesByMonthAndGroupsByRootCategory()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1));

        var incomeGroup = new Category { Name = "RENDA FAMILIAR", Type = CategoryType.Income, UserId = 1, IsActive = true };
        var expenseGroup = new Category { Name = "DESPESAS COM MORADIA", Type = CategoryType.Expense, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(incomeGroup);
        await uow.Categories.AddAsync(expenseGroup);
        await uow.SaveChangesAsync();

        var salario = new Category { Name = "Salários", Type = CategoryType.Income, ParentCategoryId = incomeGroup.Id, UserId = 1, IsActive = true };
        var aluguel = new Category { Name = "Aluguel", Type = CategoryType.Expense, ParentCategoryId = expenseGroup.Id, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(salario);
        await uow.Categories.AddAsync(aluguel);
        await uow.SaveChangesAsync();

        var account = new BankAccount { Name = "Banco 1", InitialBalance = 1000, UserId = 1, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();

        var now = DateTime.UtcNow;

        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 5),
            CategoryId = salario.Id,
            Description = "Primeira metade",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 5000,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 6),
            CategoryId = aluguel.Id,
            Description = "Pago ao Daniel",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 1500,
            Status = TransactionStatus.Pending, // não pago também entra no resumo mensal
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 2, 6), // mês diferente: deve ser ignorado
            CategoryId = aluguel.Id,
            Description = "Fevereiro",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 200,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var summary = await service.GetMonthlySummaryAsync(2026, 3);

        Assert.Equal(5000, summary.TotalIncome);
        Assert.Equal(1500, summary.TotalExpense);
        Assert.Equal(3500, summary.Balance);
        Assert.Equal(2, summary.CategoryBreakdown.Count);

        var incomeBreakdown = Assert.Single(summary.CategoryBreakdown, b => b.CategoryGroupId == incomeGroup.Id);
        Assert.Equal("RENDA FAMILIAR", incomeBreakdown.CategoryGroupName);
        Assert.Equal(5000, incomeBreakdown.Total);

        var expenseBreakdown = Assert.Single(summary.CategoryBreakdown, b => b.CategoryGroupId == expenseGroup.Id);
        Assert.Equal("DESPESAS COM MORADIA", expenseBreakdown.CategoryGroupName);
        Assert.Equal(1500, expenseBreakdown.Total);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_InvalidMonth_ThrowsBusinessRuleException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1));

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.GetMonthlySummaryAsync(2026, 13));
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_TotalBalance_SumsInitialBalancePlusPaidTransactions_AcrossAllMonths()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1));

        var incomeGroup = new Category { Name = "RENDA FAMILIAR", Type = CategoryType.Income, UserId = 1, IsActive = true };
        var expenseGroup = new Category { Name = "DESPESAS COM MORADIA", Type = CategoryType.Expense, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(incomeGroup);
        await uow.Categories.AddAsync(expenseGroup);
        await uow.SaveChangesAsync();

        var salario = new Category { Name = "Salários", Type = CategoryType.Income, ParentCategoryId = incomeGroup.Id, UserId = 1, IsActive = true };
        var aluguel = new Category { Name = "Aluguel", Type = CategoryType.Expense, ParentCategoryId = expenseGroup.Id, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(salario);
        await uow.Categories.AddAsync(aluguel);
        await uow.SaveChangesAsync();

        var contaAtivaA = new BankAccount { Name = "Banco 1", InitialBalance = 1000m, UserId = 1, IsActive = true };
        var contaAtivaB = new BankAccount { Name = "Banco 2", InitialBalance = 500m, UserId = 1, IsActive = true };
        var contaInativa = new BankAccount { Name = "Conta encerrada", InitialBalance = 9999m, UserId = 1, IsActive = false };
        await uow.BankAccounts.AddAsync(contaAtivaA);
        await uow.BankAccounts.AddAsync(contaAtivaB);
        await uow.BankAccounts.AddAsync(contaInativa);
        await uow.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // receita paga em janeiro na conta A — deve contar mesmo consultando o resumo de março
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 1, 10),
            CategoryId = salario.Id,
            Description = "Salário janeiro",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = contaAtivaA.Id,
            Amount = 300m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        // despesa paga em fevereiro na conta A
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 2, 5),
            CategoryId = aluguel.Id,
            Description = "Aluguel fevereiro",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = contaAtivaA.Id,
            Amount = 100m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        // receita paga em março na conta B
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 1),
            CategoryId = salario.Id,
            Description = "Freela março",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = contaAtivaB.Id,
            Amount = 200m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        // despesa PENDENTE em março na conta B — não deve entrar no saldo total
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 2),
            CategoryId = aluguel.Id,
            Description = "Conta a pagar",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = contaAtivaB.Id,
            Amount = 5000m,
            Status = TransactionStatus.Pending,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        // receita paga na conta INATIVA — não deve entrar no saldo total
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 3),
            CategoryId = salario.Id,
            Description = "Conta encerrada",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = contaInativa.Id,
            Amount = 7000m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var summary = await service.GetMonthlySummaryAsync(2026, 3);

        // (1000 + 500) de saldo inicial das contas ativas + (300 - 100 + 200) de lançamentos pagos
        Assert.Equal(1900m, summary.TotalBalance);
    }
}
