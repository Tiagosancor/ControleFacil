using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Microsoft.Extensions.Options;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetMonthlySummaryAsync_AggregatesByMonthAndGroupsByRootCategory()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

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
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.GetMonthlySummaryAsync(2026, 13));
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_TotalBalance_SumsInitialBalancePlusPaidTransactions_AcrossAllMonths()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

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

    [Fact]
    public async Task GetGoalComparisonAsync_NoGoalSet_ReturnsNull()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

        var result = await service.GetGoalComparisonAsync(2026, 3);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetGoalComparisonAsync_ComputesRealizedGoalAndDeviation_ForIncomeExpenseAndProfit()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

        var incomeGroup = new Category { Name = "RENDA", Type = CategoryType.Income, UserId = 1, IsActive = true };
        var expenseGroup = new Category { Name = "DESPESA", Type = CategoryType.Expense, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(incomeGroup);
        await uow.Categories.AddAsync(expenseGroup);
        await uow.SaveChangesAsync();

        var account = new BankAccount { Name = "Banco", InitialBalance = 0, UserId = 1, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();

        var now = DateTime.UtcNow;
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 5),
            CategoryId = incomeGroup.Id,
            Description = "Salário",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 4260m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 6),
            CategoryId = expenseGroup.Id,
            Description = "Aluguel",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 3000m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        await uow.MonthlyGoals.AddAsync(new MonthlyGoal
        {
            Year = 2026,
            Month = 3,
            IncomeGoal = 5000m,
            ExpenseGoal = 2000m,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var result = await service.GetGoalComparisonAsync(2026, 3);

        Assert.NotNull(result);
        Assert.Equal(4260m, result!.Income.Realized);
        Assert.Equal(5000m, result.Income.Goal);
        Assert.Equal(-14.8m, result.Income.DeviationPercent);

        Assert.Equal(3000m, result.Expense.Realized);
        Assert.Equal(2000m, result.Expense.Goal);
        Assert.Equal(50.0m, result.Expense.DeviationPercent);

        // Lucro realizado 4260-3000=1260; meta de lucro 5000-2000=3000
        Assert.Equal(1260m, result.Profit.Realized);
        Assert.Equal(3000m, result.Profit.Goal);
        Assert.Equal(-58.0m, result.Profit.DeviationPercent);
    }

    [Fact]
    public async Task GetGoalComparisonAsync_NonPositiveProfitGoal_DeviationPercentIsNull()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

        var now = DateTime.UtcNow;
        // ExpenseGoal igual a IncomeGoal -> meta de lucro é 0, dividir por zero não faz sentido.
        await uow.MonthlyGoals.AddAsync(new MonthlyGoal
        {
            Year = 2026,
            Month = 3,
            IncomeGoal = 3000m,
            ExpenseGoal = 3000m,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var result = await service.GetGoalComparisonAsync(2026, 3);

        Assert.NotNull(result);
        Assert.Null(result!.Profit.DeviationPercent);
    }

    [Fact]
    public async Task GetHistoricalSummaryAsync_ReturnsMonthsInChronologicalOrder_WithYearRollover()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

        var result = await service.GetHistoricalSummaryAsync(2026, 1, monthsBack: 3);

        Assert.Equal(4, result.Count);
        Assert.Equal((2025, 10), (result[0].Year, result[0].Month));
        Assert.Equal((2025, 11), (result[1].Year, result[1].Month));
        Assert.Equal((2025, 12), (result[2].Year, result[2].Month));
        Assert.Equal((2026, 1), (result[3].Year, result[3].Month));
    }

    [Fact]
    public async Task GetAutomaticAnalysesAsync_ExpenseExceedsIncome_IncludesNegativeAnalysis()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

        var expenseGroup = new Category { Name = "DESPESA", Type = CategoryType.Expense, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(expenseGroup);
        await uow.SaveChangesAsync();
        var account = new BankAccount { Name = "Banco", InitialBalance = 0, UserId = 1, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();

        var now = DateTime.UtcNow;
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 5),
            CategoryId = expenseGroup.Id,
            Description = "Gasto grande",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 5000m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var analyses = await service.GetAutomaticAnalysesAsync(2026, 3);

        Assert.Contains(analyses, a => a.Type == "negative");
    }

    [Fact]
    public async Task GetAutomaticAnalysesAsync_ExpenseOverGoal_IncludesWarningMentioningGoal()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new DashboardService(uow, new FakeCurrentUserService(1), Options.Create(new DueAlertOptions()));

        var expenseGroup = new Category { Name = "DESPESA", Type = CategoryType.Expense, UserId = 1, IsActive = true };
        await uow.Categories.AddAsync(expenseGroup);
        await uow.SaveChangesAsync();
        var account = new BankAccount { Name = "Banco", InitialBalance = 0, UserId = 1, IsActive = true };
        await uow.BankAccounts.AddAsync(account);
        await uow.SaveChangesAsync();

        var now = DateTime.UtcNow;
        await uow.Transactions.AddAsync(new Transaction
        {
            EntryDate = new DateOnly(2026, 3, 5),
            CategoryId = expenseGroup.Id,
            Description = "Gasto",
            PaymentMethod = PaymentMethod.Cash,
            BankAccountId = account.Id,
            Amount = 3000m,
            Status = TransactionStatus.Paid,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        await uow.MonthlyGoals.AddAsync(new MonthlyGoal
        {
            Year = 2026,
            Month = 3,
            IncomeGoal = 1m,
            ExpenseGoal = 2000m,
            UserId = 1,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await uow.SaveChangesAsync();

        var analyses = await service.GetAutomaticAnalysesAsync(2026, 3);

        Assert.Contains(analyses, a => a.Type == "warning" && a.Message.Contains("meta de despesa"));
    }
}
