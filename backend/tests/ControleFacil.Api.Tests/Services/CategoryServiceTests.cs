using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class CategoryServiceTests
{
    private static async Task<Category> SeedSystemCategoryAsync(
        IUnitOfWork uow, string name = "Alimentação", string iconKey = "utensils", string color = "#E07A5F")
    {
        var category = new Category
        {
            Name = name,
            Type = CategoryType.Expense,
            ParentCategoryId = null,
            UserId = null,
            IsSystem = true,
            IconKey = iconKey,
            Color = color,
            IsActive = true,
        };
        await uow.Categories.AddAsync(category);
        await uow.SaveChangesAsync();
        return category;
    }


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

    [Fact]
    public async Task GetAllAsync_ReturnsSystemCategoriesPlusOwnCategories_ButNotOtherUsersCategories()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var system = await SeedSystemCategoryAsync(uow);
        var serviceUser1 = new CategoryService(uow, new FakeCurrentUserService(1));
        var serviceUser2 = new CategoryService(uow, new FakeCurrentUserService(2));

        var ownUser1 = await serviceUser1.CreateAsync(new CategoryCreateDto("Categoria do usuário 1", CategoryType.Expense, null));
        await serviceUser2.CreateAsync(new CategoryCreateDto("Categoria do usuário 2", CategoryType.Expense, null));

        var result = await serviceUser1.GetAllAsync(includeInactive: false, page: 1, pageSize: 200);

        Assert.Contains(result.Items, c => c.Id == system.Id && c.IsSystem);
        Assert.Contains(result.Items, c => c.Id == ownUser1.Id);
        Assert.DoesNotContain(result.Items, c => c.Name == "Categoria do usuário 2");
    }

    [Fact]
    public async Task GetByIdAsync_SystemCategory_IsVisibleToAnyUser()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var system = await SeedSystemCategoryAsync(uow);
        var service = new CategoryService(uow, new FakeCurrentUserService(42));

        var result = await service.GetByIdAsync(system.Id);

        Assert.Equal("Alimentação", result.Name);
        Assert.True(result.IsSystem);
        Assert.Equal("utensils", result.IconKey);
        Assert.Equal("#E07A5F", result.Color);
    }

    [Fact]
    public async Task UpdateAsync_SystemCategory_ThrowsForbiddenException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var system = await SeedSystemCategoryAsync(uow);
        var service = new CategoryService(uow, new FakeCurrentUserService(1));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateAsync(system.Id, new CategoryUpdateDto("Comida", CategoryType.Expense, null, true)));
    }

    [Fact]
    public async Task DeleteAsync_SystemCategory_ThrowsForbiddenException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var system = await SeedSystemCategoryAsync(uow);
        var service = new CategoryService(uow, new FakeCurrentUserService(1));

        await Assert.ThrowsAsync<ForbiddenException>(() => service.DeleteAsync(system.Id));
    }

    [Fact]
    public async Task CreateAsync_SubcategoryUnderSystemParent_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var system = await SeedSystemCategoryAsync(uow);
        var service = new CategoryService(uow, new FakeCurrentUserService(1));

        var sub = await service.CreateAsync(new CategoryCreateDto("Padaria", CategoryType.Income, system.Id));

        Assert.Equal(system.Id, sub.ParentCategoryId);
        Assert.Equal(CategoryType.Expense, sub.Type); // herda do pai de sistema
        Assert.False(sub.IsSystem); // a subcategoria em si continua sendo do usuário
    }
}
