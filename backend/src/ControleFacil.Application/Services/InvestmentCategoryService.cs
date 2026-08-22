using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class InvestmentCategoryService : IInvestmentCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public InvestmentCategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InvestmentCategoryResponseDto>> GetAllAsync(bool includeInactive)
    {
        var query = _unitOfWork.InvestmentCategories.Query().Where(c => c.UserId == _currentUser.UserId);
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query.OrderBy(c => c.Name).ToListAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task<InvestmentCategoryResponseDto> GetByIdAsync(int id)
    {
        var category = await GetOwnedAsync(id);
        return ToDto(category);
    }

    public async Task<InvestmentCategoryResponseDto> CreateAsync(InvestmentCategoryCreateDto dto)
    {
        var category = new InvestmentCategory
        {
            Name = dto.Name,
            UserId = _currentUser.UserId,
            IsActive = true,
        };

        await _unitOfWork.InvestmentCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task<InvestmentCategoryResponseDto> UpdateAsync(int id, InvestmentCategoryUpdateDto dto)
    {
        var category = await GetOwnedAsync(id);

        category.Name = dto.Name;
        category.IsActive = dto.IsActive;

        _unitOfWork.InvestmentCategories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await GetOwnedAsync(id);
        category.IsActive = false;
        _unitOfWork.InvestmentCategories.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<InvestmentCategory> GetOwnedAsync(int id)
    {
        var category = await _unitOfWork.InvestmentCategories.Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _currentUser.UserId);

        return category ?? throw new NotFoundException("Categoria de investimento não encontrada.");
    }

    private static InvestmentCategoryResponseDto ToDto(InvestmentCategory c) => new(c.Id, c.Name, c.IsActive);
}
