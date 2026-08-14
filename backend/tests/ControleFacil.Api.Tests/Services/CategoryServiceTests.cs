using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Enums;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_ChildCategory_InheritsParentType()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryService(uow, new FakeCurrentUserService(1));

        var parent = await service.CreateAsync(new CategoryCreateDto("Renda Familiar", CategoryType.Income, null));

        // deliberadamente enviando Type=Expense para a subcategoria: deve ser ignorado e herdar Income do pai
        var child = await service.CreateAsync(new CategoryCreateDto("Salários", CategoryType.Expense, parent.Id));

        Assert.Equal(CategoryType.Income, child.Type);
        Assert.Equal(parent.Id, child.ParentCategoryId);
    }

    [Fact]
    public async Task UpdateAsync_ChildCategory_InheritsParentTypeOnUpdate()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryService(uow, new FakeCurrentUserService(1));

        var parent = await service.CreateAsync(new CategoryCreateDto("Despesas com Moradia", CategoryType.Expense, null));
        var child = await service.CreateAsync(new CategoryCreateDto("Aluguel", CategoryType.Expense, parent.Id));

        var updated = await service.UpdateAsync(
            child.Id,
            new CategoryUpdateDto("Aluguel", CategoryType.Income, parent.Id, true));

        Assert.Equal(CategoryType.Expense, updated.Type);
    }

    [Fact]
    public async Task CreateAsync_ThreeLevelHierarchy_ThrowsBusinessRuleException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var service = new CategoryService(uow, new FakeCurrentUserService(1));

        var group = await service.CreateAsync(new CategoryCreateDto("Grupo", CategoryType.Expense, null));
        var subcategory = await service.CreateAsync(new CategoryCreateDto("Subcategoria", CategoryType.Expense, group.Id));

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.CreateAsync(new CategoryCreateDto("Neto", CategoryType.Expense, subcategory.Id)));
    }
}
