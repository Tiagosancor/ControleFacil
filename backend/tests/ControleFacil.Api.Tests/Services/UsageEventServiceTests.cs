using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class UsageEventServiceTests
{
    private static IConfiguration BuildConfig(string? expireMinutes = "120") => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:ExpireMinutes"] = expireMinutes })
        .Build();

    private static async Task<(User user1, User user2)> SeedUsersAsync(ControleFacil.Domain.Interfaces.IUnitOfWork uow)
    {
        var user1 = new User { Name = "Ana", Email = "ana@teste.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Name = "Bruno", Email = "bruno@teste.com", PasswordHash = "x", CreatedAt = DateTime.UtcNow };
        await uow.Users.AddAsync(user1);
        await uow.Users.AddAsync(user2);
        await uow.SaveChangesAsync();
        return (user1, user2);
    }

    [Fact]
    public async Task GetLoginHistoryAsync_ReturnsOnlyLoginEvents_MostRecentFirst()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new UsageEventService(uow, BuildConfig());
        var (user1, _) = await SeedUsersAsync(uow);

        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc) });
        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.DashboardAcessado, CreatedAt = new DateTime(2026, 1, 1, 10, 5, 0, DateTimeKind.Utc) });
        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc) });
        await uow.SaveChangesAsync();

        var result = await service.GetLoginHistoryAsync(userId: null, page: 1, pageSize: 20);

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new DateTime(2026, 1, 2, 9, 0, 0, DateTimeKind.Utc), result.Items[0].CreatedAt);
        Assert.Equal("Ana", result.Items[0].UserName);
    }

    [Fact]
    public async Task GetLoginHistoryAsync_FiltersByUserId()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new UsageEventService(uow, BuildConfig());
        var (user1, user2) = await SeedUsersAsync(uow);

        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = DateTime.UtcNow });
        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user2.Id, EventType = UsageEventType.Login, CreatedAt = DateTime.UtcNow });
        await uow.SaveChangesAsync();

        var result = await service.GetLoginHistoryAsync(userId: user2.Id, page: 1, pageSize: 20);

        var item = Assert.Single(result.Items);
        Assert.Equal("Bruno", item.UserName);
    }

    [Fact]
    public async Task GetLoginHistoryAsync_Paginates()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new UsageEventService(uow, BuildConfig());
        var (user1, _) = await SeedUsersAsync(uow);

        for (var i = 0; i < 5; i++)
        {
            await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = DateTime.UtcNow.AddMinutes(i) });
        }
        await uow.SaveChangesAsync();

        var result = await service.GetLoginHistoryAsync(userId: null, page: 1, pageSize: 2);

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetLoggedInUsersAsync_ExcludesLoginsOutsideWindow()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new UsageEventService(uow, BuildConfig());
        var (user1, user2) = await SeedUsersAsync(uow);

        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = DateTime.UtcNow.AddMinutes(-10) }); // dentro
        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user2.Id, EventType = UsageEventType.Login, CreatedAt = DateTime.UtcNow.AddDays(-5) }); // fora
        await uow.SaveChangesAsync();

        var result = await service.GetLoggedInUsersAsync(minutes: 30);

        var item = Assert.Single(result);
        Assert.Equal("Ana", item.UserName);
    }

    [Fact]
    public async Task GetLoggedInUsersAsync_DedupesToLatestLoginPerUser()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new UsageEventService(uow, BuildConfig());
        var (user1, _) = await SeedUsersAsync(uow);

        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = DateTime.UtcNow.AddMinutes(-20) });
        var latest = DateTime.UtcNow.AddMinutes(-5);
        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = latest });
        await uow.SaveChangesAsync();

        var result = await service.GetLoggedInUsersAsync(minutes: 30);

        var item = Assert.Single(result);
        Assert.Equal(latest, item.LastLoginAt);
    }

    [Fact]
    public async Task GetLoggedInUsersAsync_NoOverride_UsesJwtExpireMinutesAsDefaultWindow()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new UsageEventService(uow, BuildConfig(expireMinutes: "15"));
        var (user1, _) = await SeedUsersAsync(uow);

        await uow.UsageEvents.AddAsync(new UsageEvent { UserId = user1.Id, EventType = UsageEventType.Login, CreatedAt = DateTime.UtcNow.AddMinutes(-20) }); // fora da janela de 15min
        await uow.SaveChangesAsync();

        var result = await service.GetLoggedInUsersAsync(minutes: null);

        Assert.Empty(result);
    }
}
