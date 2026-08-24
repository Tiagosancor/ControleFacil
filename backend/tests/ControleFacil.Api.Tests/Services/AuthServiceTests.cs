using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(User user, string password) => $"hash:{password}";
    public bool VerifyPassword(User user, string password) => user.PasswordHash == $"hash:{password}";
}

public class FakeJwtTokenService : IJwtTokenService
{
    public string GenerateToken(User user) => $"fake-token-for-{user.Id}";
}

public class AuthServiceTests
{
    private static AuthService BuildService(ControleFacil.Domain.Interfaces.IUnitOfWork uow) => new(
        uow,
        new FakePasswordHasher(),
        new FakeJwtTokenService(),
        new FakeEmailService(),
        new ConfigurationBuilder().Build(),
        NullLogger<AuthService>.Instance);

    [Fact]
    public async Task LoginAsync_RecordsLoginUsageEvent()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = BuildService(uow);

        var user = new User { Name = "Ana", Email = "ana@teste.com", CreatedAt = DateTime.UtcNow };
        user.PasswordHash = "hash:Senha123!";
        await uow.Users.AddAsync(user);
        await uow.SaveChangesAsync();

        await service.LoginAsync(new LoginDto("ana@teste.com", "Senha123!"));

        var events = uow.UsageEvents.Query().Where(e => e.UserId == user.Id).ToList();
        var loginEvent = Assert.Single(events);
        Assert.Equal(UsageEventType.Login, loginEvent.EventType);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_DoesNotRecordUsageEvent()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = BuildService(uow);

        var user = new User { Name = "Ana", Email = "ana@teste.com", CreatedAt = DateTime.UtcNow };
        user.PasswordHash = "hash:Senha123!";
        await uow.Users.AddAsync(user);
        await uow.SaveChangesAsync();

        await Assert.ThrowsAnyAsync<Exception>(() => service.LoginAsync(new LoginDto("ana@teste.com", "SenhaErrada")));

        Assert.Empty(uow.UsageEvents.Query().Where(e => e.UserId == user.Id));
    }
}
