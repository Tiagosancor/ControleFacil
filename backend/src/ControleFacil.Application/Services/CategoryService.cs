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
        var query = _unitOfWork.Categories.QueryWithDetails().Where(c => c.UserId == _currentUser.UserId || c.IsSystem);
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
        var category = await GetVisibleAsync(id);
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
        var category = await GetOwnedForMutationAsync(id);

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
        var category = await GetOwnedForMutationAsync(id);
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

        // O pai pode ser uma categoria de sistema (ex.: subcategoria "Padaria" do
        // usuário, filha de "Alimentação" do sistema) — só a visibilidade importa aqui,
        // não a posse, já que criar um filho não altera a categoria pai em si.
        var parent = await GetVisibleAsync(parentCategoryId.Value);
        if (parent.ParentCategoryId.HasValue)
            throw new BusinessRuleException("A categoria pai já é uma subcategoria; a hierarquia suporta apenas 2 níveis.");

        category.ParentCategoryId = parent.Id;
        category.Type = parent.Type; // a subcategoria sempre herda o Type do grupo pai
    }

    // Leitura: própria do usuário OU de sistema. Uma categoria de outro usuário não
    // aparece aqui — cai no NotFoundException, mesmo padrão "404 em vez de 403" já usado
    // no projeto pra não vazar a existência de recursos de outro usuário.
    private async Task<Category> GetVisibleAsync(int id)
    {
        var category = await _unitOfWork.Categories.QueryWithDetails()
            .FirstOrDefaultAsync(c => c.Id == id && (c.UserId == _currentUser.UserId || c.IsSystem));

        return category ?? throw new NotFoundException("Categoria não encontrada.");
    }

    // Escrita: além de visível, precisa ser própria do usuário — categoria de sistema é
    // visível mas nunca editável/excluível, daí o 403 (ForbiddenException) explícito em
    // vez de reaproveitar o NotFoundException da leitura.
    private async Task<Category> GetOwnedForMutationAsync(int id)
    {
        var category = await GetVisibleAsync(id);
        if (category.IsSystem)
            throw new ForbiddenException("Categorias de sistema não podem ser editadas ou excluídas.");

        return category;
    }

    private static CategoryResponseDto ToDto(Category c) => new(
        c.Id,
        c.Name,
        c.Type,
        c.ParentCategoryId,
        c.ParentCategory?.Name,
        c.IsActive,
        c.IsSystem,
        c.IconKey,
        c.Color);
}
