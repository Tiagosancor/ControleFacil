using ControleFacil.Api.Tests.TestHelpers;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Services;
using Xunit;

namespace ControleFacil.Api.Tests.Services;

public class InvestmentEntryServiceTests
{
    private static async Task<int> CreateCategoryAsync(InvestmentCategoryService categoryService, string name = "Renda Fixa")
    {
        var category = await categoryService.CreateAsync(new InvestmentCategoryCreateDto(name));
        return category.Id;
    }

    [Fact]
    public async Task CreateAsync_ValidEntry_Succeeds()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var currentUser = new FakeCurrentUserService(1);
        var categoryService = new InvestmentCategoryService(uow, currentUser);
        var entryService = new InvestmentEntryService(uow, currentUser);
        var categoryId = await CreateCategoryAsync(categoryService);

        var result = await entryService.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 3, 12400m));

        Assert.Equal(12400m, result.Value);
        Assert.Equal("Renda Fixa", result.InvestmentCategoryName);
    }

    [Fact]
    public async Task CreateAsync_DuplicateForSameCategoryAndMonth_ThrowsConflictException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var currentUser = new FakeCurrentUserService(1);
        var categoryService = new InvestmentCategoryService(uow, currentUser);
        var entryService = new InvestmentEntryService(uow, currentUser);
        var categoryId = await CreateCategoryAsync(categoryService);

        await entryService.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 3, 12400m));

        await Assert.ThrowsAsync<ConflictException>(() =>
            entryService.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 3, 13000m)));
    }

    [Fact]
    public async Task CreateAsync_CategoryFromAnotherUser_ThrowsNotFoundException()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var categoryService = new InvestmentCategoryService(uow, new FakeCurrentUserService(1));
        var categoryId = await CreateCategoryAsync(categoryService);

        var entryServiceUser2 = new InvestmentEntryService(uow, new FakeCurrentUserService(2));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            entryServiceUser2.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 3, 1000m)));
    }

    [Fact]
    public async Task UpdateAsync_ChangesValue()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var currentUser = new FakeCurrentUserService(1);
        var categoryService = new InvestmentCategoryService(uow, currentUser);
        var entryService = new InvestmentEntryService(uow, currentUser);
        var categoryId = await CreateCategoryAsync(categoryService);
        var created = await entryService.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 3, 12400m));

        var updated = await entryService.UpdateAsync(created.Id, new InvestmentEntryUpdateDto(categoryId, 2026, 3, 13100m));

        Assert.Equal(13100m, updated.Value);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntry()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var currentUser = new FakeCurrentUserService(1);
        var categoryService = new InvestmentCategoryService(uow, currentUser);
        var entryService = new InvestmentEntryService(uow, currentUser);
        var categoryId = await CreateCategoryAsync(categoryService);
        var created = await entryService.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 3, 12400m));

        await entryService.DeleteAsync(created.Id);

        var remaining = await entryService.GetAllAsync(2026, 3);
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByYearAndMonth()
    {
        var uow = TestUnitOfWorkFactory.Create(out _);
        var currentUser = new FakeCurrentUserService(1);
        var categoryService = new InvestmentCategoryService(uow, currentUser);
        var entryService = new InvestmentEntryService(uow, currentUser);
        var categoryId = await CreateCategoryAsync(categoryService);
        await entryService.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 3, 12400m));
        await entryService.CreateAsync(new InvestmentEntryCreateDto(categoryId, 2026, 4, 12900m));

        var marchOnly = await entryService.GetAllAsync(2026, 3);

        var entry = Assert.Single(marchOnly);
        Assert.Equal(3, entry.Month);
    }
}
