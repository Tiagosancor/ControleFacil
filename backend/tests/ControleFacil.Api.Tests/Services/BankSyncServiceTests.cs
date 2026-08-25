using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class FakeBrasilApiBankClient : IBrasilApiBankClient
{
    public List<BankSyncItemDto> Items { get; set; } = new();

    public Task<IReadOnlyList<BankSyncItemDto>> FetchAllAsync() =>
        Task.FromResult<IReadOnlyList<BankSyncItemDto>>(Items);
}

public class BankSyncServiceTests
{
    [Fact]
    public async Task SyncAsync_NoExistingBanks_InsertsAll()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var client = new FakeBrasilApiBankClient
        {
            Items = new List<BankSyncItemDto>
            {
                new("00000000", 1, "BCO DO BRASIL S.A.", "Banco do Brasil S.A.", "https://x/bb.svg"),
                new("60701190", 341, "ITAU UNIBANCO S.A.", "Itaú Unibanco S.A.", "https://x/itau.svg"),
            },
        };
        var service = new BankSyncService(uow, client);

        var count = await service.SyncAsync();

        Assert.Equal(2, count);
        var banks = uow.Banks.Query().ToList();
        Assert.Equal(2, banks.Count);
    }

    [Fact]
    public async Task SyncAsync_ExistingBank_UpdatesInPlace_DoesNotDuplicate()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await uow.Banks.AddAsync(new Bank { Ispb = "00000000", Code = 1, Name = "Nome Antigo", FullName = "Full Antigo", UpdatedAt = DateTime.UtcNow.AddDays(-30) });
        await uow.SaveChangesAsync();

        var client = new FakeBrasilApiBankClient
        {
            Items = new List<BankSyncItemDto> { new("00000000", 1, "BCO DO BRASIL S.A.", "Banco do Brasil S.A.", "https://x/bb.svg") },
        };
        var service = new BankSyncService(uow, client);

        await service.SyncAsync();

        var banks = uow.Banks.Query().ToList();
        var bank = Assert.Single(banks);
        Assert.Equal("BCO DO BRASIL S.A.", bank.Name);
        Assert.Equal("https://x/bb.svg", bank.LogoUrl);
    }

    [Fact]
    public async Task SyncAsync_ClientReturnsEmpty_DoesNothing_NoException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var client = new FakeBrasilApiBankClient { Items = new List<BankSyncItemDto>() };
        var service = new BankSyncService(uow, client);

        var count = await service.SyncAsync();

        Assert.Equal(0, count);
        Assert.Empty(uow.Banks.Query().ToList());
    }
}
