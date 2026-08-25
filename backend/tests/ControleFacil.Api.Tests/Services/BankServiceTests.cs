using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class BankServiceTests
{
    private static async Task SeedBanksAsync(ControleFacil.Domain.Interfaces.IUnitOfWork uow)
    {
        await uow.Banks.AddAsync(new Bank { Ispb = "00000000", Code = 1, Name = "BCO DO BRASIL S.A.", FullName = "Banco do Brasil S.A.", LogoUrl = "https://x/bb.svg", UpdatedAt = DateTime.UtcNow });
        await uow.Banks.AddAsync(new Bank { Ispb = "60701190", Code = 341, Name = "ITAU UNIBANCO S.A.", FullName = "Itaú Unibanco S.A.", LogoUrl = "https://x/itau.svg", UpdatedAt = DateTime.UtcNow });
        await uow.Banks.AddAsync(new Bank { Ispb = "18236120", Code = 260, Name = "NU PAGAMENTOS S.A.", FullName = "Nu Pagamentos S.A.", LogoUrl = null, UpdatedAt = DateTime.UtcNow });
        await uow.SaveChangesAsync();
    }

    [Fact]
    public async Task SearchAsync_NoTerm_ReturnsAllUpToDefaultLimit()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await SeedBanksAsync(uow);
        var service = new BankService(uow);

        var result = await service.SearchAsync(null);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_FiltersCaseInsensitiveBySubstring()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await SeedBanksAsync(uow);
        var service = new BankService(uow);

        var result = await service.SearchAsync("itau");

        var item = Assert.Single(result);
        Assert.Equal("60701190", item.Ispb);
    }

    [Fact]
    public async Task SearchAsync_MatchesFullNameToo()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await SeedBanksAsync(uow);
        var service = new BankService(uow);

        var result = await service.SearchAsync("nu pagamentos");

        Assert.Single(result);
    }

    [Fact]
    public async Task SearchAsync_IgnoresAccents_UnaccentedSearchMatchesAccentedName()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await uow.Banks.AddAsync(new Bank { Ispb = "60701190", Code = 341, Name = "ITAÚ UNIBANCO S.A.", FullName = "Itaú Unibanco S.A.", UpdatedAt = DateTime.UtcNow });
        await uow.SaveChangesAsync();
        var service = new BankService(uow);

        var result = await service.SearchAsync("itau");

        var item = Assert.Single(result);
        Assert.Equal("60701190", item.Ispb);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        await SeedBanksAsync(uow);
        var service = new BankService(uow);

        var result = await service.SearchAsync("banco-que-nao-existe");

        Assert.Empty(result);
    }
}
