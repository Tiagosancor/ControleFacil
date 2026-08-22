using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class InvestmentCategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidCategory_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new InvestmentCategoryCreateDto("Renda Fixa"));

        Assert.Equal("Renda Fixa", result.Name);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesInactiveByDefault()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new InvestmentCategoryCreateDto("Ações"));
        await service.UpdateAsync(created.Id, new InvestmentCategoryUpdateDto("Ações", false));

        var active = await service.GetAllAsync(includeInactive: false);
        var all = await service.GetAllAsync(includeInactive: true);

        Assert.Empty(active);
        Assert.Single(all);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes_SetsIsActiveFalse()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new InvestmentCategoryCreateDto("Cripto"));

        await service.DeleteAsync(created.Id);
        var reloaded = await service.GetByIdAsync(created.Id);

        Assert.False(reloaded.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_AnotherUsersCategory_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUser1 = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));
        var created = await serviceUser1.CreateAsync(new InvestmentCategoryCreateDto("Fundos Imobiliários"));

        var serviceUser2 = new InvestmentCategoryService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUser2.GetByIdAsync(created.Id));
    }
}
