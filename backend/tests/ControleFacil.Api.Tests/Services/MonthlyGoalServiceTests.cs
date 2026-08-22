using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class MonthlyGoalServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidGoal_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new MonthlyGoalService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));

        Assert.Equal(2026, result.Year);
        Assert.Equal(3, result.Month);
        Assert.Equal(8000m, result.IncomeGoal);
        Assert.Equal(6000m, result.ExpenseGoal);
    }

    [Fact]
    public async Task CreateAsync_DuplicateForSameMonth_ThrowsConflictException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new MonthlyGoalService(uow, new FakeCurrentUserService(1));

        await service.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 9000m, 7000m)));
    }

    [Fact]
    public async Task CreateAsync_SameMonthDifferentUser_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUser1 = new MonthlyGoalService(uow, new FakeCurrentUserService(1));
        var serviceUser2 = new MonthlyGoalService(uow, new FakeCurrentUserService(2));

        await serviceUser1.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));
        var result = await serviceUser2.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 5000m, 4000m));

        Assert.Equal(5000m, result.IncomeGoal);
    }

    [Fact]
    public async Task UpdateAsync_ChangesFields()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new MonthlyGoalService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));

        var updated = await service.UpdateAsync(created.Id, new MonthlyGoalUpdateDto(2026, 3, 9000m, 6500m));

        Assert.Equal(9000m, updated.IncomeGoal);
        Assert.Equal(6500m, updated.ExpenseGoal);
    }

    [Fact]
    public async Task UpdateAsync_ToAnotherMonthAlreadyTaken_ThrowsConflictException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new MonthlyGoalService(uow, new FakeCurrentUserService(1));
        await service.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));
        var april = await service.CreateAsync(new MonthlyGoalCreateDto(2026, 4, 8000m, 6000m));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(april.Id, new MonthlyGoalUpdateDto(2026, 3, 8000m, 6000m)));
    }

    [Fact]
    public async Task DeleteAsync_RemovesGoal()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new MonthlyGoalService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));

        await service.DeleteAsync(created.Id);

        var remaining = await service.GetAllAsync(2026, 3);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task GetByIdAsync_AnotherUsersGoal_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUser1 = new MonthlyGoalService(uow, new FakeCurrentUserService(1));
        var created = await serviceUser1.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));

        var serviceUser2 = new MonthlyGoalService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUser2.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByYearAndMonth()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new MonthlyGoalService(uow, new FakeCurrentUserService(1));
        await service.CreateAsync(new MonthlyGoalCreateDto(2026, 3, 8000m, 6000m));
        await service.CreateAsync(new MonthlyGoalCreateDto(2026, 4, 8500m, 6200m));

        var marchOnly = await service.GetAllAsync(2026, 3);

        var goal = Assert.Single(marchOnly);
        Assert.Equal(3, goal.Month);
    }
}
