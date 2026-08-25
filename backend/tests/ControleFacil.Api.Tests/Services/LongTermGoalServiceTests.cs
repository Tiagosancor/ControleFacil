using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class LongTermGoalServiceTests
{
    [Fact]
    public async Task CreateAsync_ManualAmount_ComputesProgressAndMonthlyContribution()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new LongTermGoalService(uow, new FakeCurrentUserService(1));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var targetYear = today.Year;
        var targetMonth = today.Month + 4 > 12 ? today.Month + 4 - 12 : today.Month + 4;
        if (today.Month + 4 > 12) targetYear++;

        var result = await service.CreateAsync(new LongTermGoalCreateDto(
            "Carro", 10000m, targetYear, targetMonth, InvestmentCategoryId: null, ManualCurrentAmount: 2000m));

        Assert.Equal("Carro", result.Name);
        Assert.Equal(2000m, result.CurrentAmount);
        Assert.Equal(0.2m, result.ProgressPercentage);
        Assert.Equal(4, result.MonthsRemaining);
        Assert.Equal(2000m, result.MonthlyContributionNeeded); // (10000-2000)/4
    }

    [Fact]
    public async Task CreateAsync_TargetInThePast_ThrowsBusinessRuleException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new LongTermGoalService(uow, new FakeCurrentUserService(1));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(new LongTermGoalCreateDto("Casa", 50000m, 2000, 1, null, 0m)));
    }

    [Fact]
    public async Task CreateAsync_GoalAlreadyReached_MonthlyContributionIsZero()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new LongTermGoalService(uow, new FakeCurrentUserService(1));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var future = today.AddMonths(6);

        var result = await service.CreateAsync(new LongTermGoalCreateDto(
            "Viagem", 5000m, future.Year, future.Month, InvestmentCategoryId: null, ManualCurrentAmount: 6000m));

        Assert.Equal(0m, result.MonthlyContributionNeeded);
        Assert.True(result.ProgressPercentage >= 1m);
    }

    [Fact]
    public async Task CreateAsync_LinkedToInvestmentCategory_UsesLatestEntryAsCurrentAmount()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var currentUser = new FakeCurrentUserService(1);
        var categoryService = new InvestmentCategoryService(uow, currentUser);
        var entryService = new InvestmentEntryService(uow, currentUser);
        var goalService = new LongTermGoalService(uow, currentUser);

        var category = await categoryService.CreateAsync(new InvestmentCategoryCreateDto("Reserva Carro", InvestmentType.CDB, 1000m));
        await entryService.CreateAsync(new InvestmentEntryCreateDto(category.Id, 2026, 1, 3000m));
        await entryService.CreateAsync(new InvestmentEntryCreateDto(category.Id, 2026, 2, 3500m));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var future = today.AddMonths(3);

        var result = await goalService.CreateAsync(new LongTermGoalCreateDto(
            "Carro", 10000m, future.Year, future.Month, category.Id, ManualCurrentAmount: 0m));

        // Deve usar o lançamento mais recente (fevereiro = 3500), ignorando o manual (0) e o de janeiro.
        Assert.Equal(3500m, result.CurrentAmount);
        Assert.Equal("Reserva Carro", result.InvestmentCategoryName);
    }

    [Fact]
    public async Task CreateAsync_InvestmentCategoryFromAnotherUser_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var categoryService = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));
        var category = await categoryService.CreateAsync(new InvestmentCategoryCreateDto("Renda Fixa", InvestmentType.CDB, 1000m));

        var goalServiceUser2 = new LongTermGoalService(uow, new FakeCurrentUserService(2));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var future = today.AddMonths(3);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            goalServiceUser2.CreateAsync(new LongTermGoalCreateDto("Casa", 10000m, future.Year, future.Month, category.Id, 0m)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesGoal()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new LongTermGoalService(uow, new FakeCurrentUserService(1));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var future = today.AddMonths(3);
        var created = await service.CreateAsync(new LongTermGoalCreateDto("Viagem", 5000m, future.Year, future.Month, null, 0m));

        await service.DeleteAsync(created.Id);

        var remaining = await service.GetAllAsync();
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task GetByIdAsync_AnotherUsersGoal_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUser1 = new LongTermGoalService(uow, new FakeCurrentUserService(1));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var future = today.AddMonths(3);
        var created = await serviceUser1.CreateAsync(new LongTermGoalCreateDto("Casa", 50000m, future.Year, future.Month, null, 0m));

        var serviceUser2 = new LongTermGoalService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUser2.GetByIdAsync(created.Id));
    }
}
