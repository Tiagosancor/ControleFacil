using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class BankAccountServiceTests
{
    private static async Task SeedBankAsync(ControleFacil.Domain.Interfaces.IUnitOfWork uow, string ispb = "60701190")
    {
        await uow.Banks.AddAsync(new Bank { Ispb = ispb, Code = 341, Name = "ITAU UNIBANCO S.A.", FullName = "Itaú Unibanco S.A.", LogoUrl = "https://x/itau.svg", UpdatedAt = DateTime.UtcNow });
        await uow.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateAsync_WithoutBankIspb_NameStaysFreeText_BankIsNull()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new BankAccountService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new BankAccountCreateDto("Caixinha", 200m));

        Assert.Equal("Caixinha", result.Name);
        Assert.Null(result.Bank);
    }

    [Fact]
    public async Task CreateAsync_WithValidBankIspb_ReturnsBankNameAndLogo()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await SeedBankAsync(uow);
        var service = new BankAccountService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new BankAccountCreateDto("Conta salário", 1000m, "60701190"));

        Assert.NotNull(result.Bank);
        Assert.Equal("ITAU UNIBANCO S.A.", result.Bank!.Name);
        Assert.Equal("https://x/itau.svg", result.Bank.LogoUrl);
        Assert.Equal("Conta salário", result.Name); // apelido continua livre, independente do banco
    }

    [Fact]
    public async Task CreateAsync_WithUnknownBankIspb_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new BankAccountService(uow, new FakeCurrentUserService(1));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(new BankAccountCreateDto("Conta X", 100m, "99999999")));
    }

    [Fact]
    public async Task UpdateAsync_ClearingBankIspb_RemovesBankFromResponse()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await SeedBankAsync(uow);
        var service = new BankAccountService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new BankAccountCreateDto("Conta salário", 1000m, "60701190"));

        var updated = await service.UpdateAsync(created.Id, new BankAccountUpdateDto("Conta salário", 1000m, true, null));

        Assert.Null(updated.Bank);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsBankInfo_AfterCreatedWithBank()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await SeedBankAsync(uow);
        var service = new BankAccountService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new BankAccountCreateDto("Conta salário", 1000m, "60701190"));

        var reloaded = await service.GetByIdAsync(created.Id);

        Assert.NotNull(reloaded.Bank);
        Assert.Equal("60701190", reloaded.Bank!.Ispb);
    }
}
