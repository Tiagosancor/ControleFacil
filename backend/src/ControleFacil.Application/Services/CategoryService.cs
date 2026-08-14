using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResultDto<CategoryResponseDto>> GetAllAsync(bool includeInactive, int page, int pageSize)
    {
        var query = _unitOfWork.Categories.QueryWithDetails().Where(c => c.UserId == _currentUser.UserId);
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<CategoryResponseDto>(total, page, pageSize, items.Select(ToDto).ToList());
    }

    public async Task<CategoryResponseDto> GetByIdAsync(int id)
    {
        var category = await GetOwnedAsync(id);
        return ToDto(category);
    }

    public async Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            UserId = _currentUser.UserId,
            IsActive = true,
        };

        await ApplyParentAndTypeAsync(category, dto.ParentCategoryId, dto.Type);

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task<CategoryResponseDto> UpdateAsync(int id, CategoryUpdateDto dto)
    {
        var category = await GetOwnedAsync(id);

        if (dto.ParentCategoryId == id)
            throw new BusinessRuleException("Uma categoria não pode ser pai dela mesma.");

        category.Name = dto.Name;
        category.IsActive = dto.IsActive;
        await ApplyParentAndTypeAsync(category, dto.ParentCategoryId, dto.Type);

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await GetOwnedAsync(id);
        category.IsActive = false;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task ApplyParentAndTypeAsync(Category category, int? parentCategoryId, Domain.Enums.CategoryType requestedType)
    {
        if (parentCategoryId is null)
        {
            category.ParentCategoryId = null;
            category.Type = requestedType;
            return;
        }

        var parent = await GetOwnedAsync(parentCategoryId.Value);
        if (parent.ParentCategoryId.HasValue)
            throw new BusinessRuleException("A categoria pai já é uma subcategoria; a hierarquia suporta apenas 2 níveis.");

        category.ParentCategoryId = parent.Id;
        category.Type = parent.Type; // a subcategoria sempre herda o Type do grupo pai
    }

    private async Task<Category> GetOwnedAsync(int id)
    {
        var category = await _unitOfWork.Categories.QueryWithDetails()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _currentUser.UserId);

        return category ?? throw new NotFoundException("Categoria não encontrada.");
    }

    private static CategoryResponseDto ToDto(Category c) => new(
        c.Id,
        c.Name,
        c.Type,
        c.ParentCategoryId,
        c.ParentCategory?.Name,
        c.IsActive);
}
