using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class InvestmentCategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidCategory_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new InvestmentCategoryCreateDto("Renda Fixa", InvestmentType.CDB, 1000m, InterestRate: 12.5m));

        Assert.Equal("Renda Fixa", result.Name);
        Assert.Equal(InvestmentType.CDB, result.Type);
        Assert.Equal(InvestmentGroup.RendaFixa, result.Group);
        Assert.Equal(1000m, result.AppliedAmount);
        Assert.Equal(12.5m, result.InterestRate);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_DerivesGroupFromType()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new InvestmentCategoryCreateDto("Minhas Ações", InvestmentType.Acoes, 500m));

        Assert.Equal(InvestmentGroup.RendaVariavel, result.Group);
        Assert.Null(result.InterestRate);
    }

    [Fact]
    public async Task CreateAsync_InterestRateOnGroupWithoutIt_ThrowsBusinessRuleException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(new InvestmentCategoryCreateDto("Ações", InvestmentType.Acoes, 500m, InterestRate: 5m)));
    }

    [Fact]
    public async Task CreateAsync_InterestRateOnPrevidenciaPrivada_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));

        var result = await service.CreateAsync(new InvestmentCategoryCreateDto("Previdência", InvestmentType.VGBL, 2000m, InterestRate: 4.2m));

        Assert.Equal(InvestmentGroup.PrevidenciaPrivada, result.Group);
        Assert.Equal(4.2m, result.InterestRate);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesInactiveByDefault()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));
        var created = await service.CreateAsync(new InvestmentCategoryCreateDto("Ações", InvestmentType.Acoes, 500m));
        await service.UpdateAsync(created.Id, new InvestmentCategoryUpdateDto("Ações", InvestmentType.Acoes, 500m, IsActive: false));

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
        var created = await service.CreateAsync(new InvestmentCategoryCreateDto("Cripto", InvestmentType.Criptomoeda, 300m));

        await service.DeleteAsync(created.Id);
        var reloaded = await service.GetByIdAsync(created.Id);

        Assert.False(reloaded.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_AnotherUsersCategory_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var serviceUser1 = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));
        var created = await serviceUser1.CreateAsync(new InvestmentCategoryCreateDto("Fundos Imobiliários", InvestmentType.FII, 800m));

        var serviceUser2 = new InvestmentCategoryService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() => serviceUser2.GetByIdAsync(created.Id));
    }
}
