using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
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
        EnsureInterestRateAllowed(dto.Type, dto.InterestRate);

        var category = new InvestmentCategory
        {
            Name = dto.Name,
            Type = dto.Type,
            AppliedAmount = dto.AppliedAmount,
            InterestRate = dto.InterestRate,
            MonthlyContribution = dto.MonthlyContribution,
            UserId = _currentUser.UserId,
            IsActive = true,
        };

        await _unitOfWork.InvestmentCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(category);
    }

    public async Task<InvestmentCategoryResponseDto> UpdateAsync(int id, InvestmentCategoryUpdateDto dto)
    {
        EnsureInterestRateAllowed(dto.Type, dto.InterestRate);

        var category = await GetOwnedAsync(id);

        category.Name = dto.Name;
        category.Type = dto.Type;
        category.AppliedAmount = dto.AppliedAmount;
        category.InterestRate = dto.InterestRate;
        category.MonthlyContribution = dto.MonthlyContribution;
        category.IsActive = dto.IsActive;

        _unitOfWork.InvestmentCategories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(category);
    }

    // Taxa de juros só existe pra quem tem remuneração contratada (Renda Fixa) ou algo
    // análogo (Previdência Privada) — evita salvar uma taxa sem sentido pra Ações, por
    // exemplo, mesmo que o cliente da API tente enviar uma.
    private static void EnsureInterestRateAllowed(InvestmentType type, decimal? interestRate)
    {
        if (!interestRate.HasValue) return;

        var group = InvestmentTypeCatalog.GroupOf[type];
        if (!InvestmentTypeCatalog.GroupsWithInterestRate.Contains(group))
            throw new BusinessRuleException("Taxa de juros só se aplica a Renda Fixa e Previdência Privada.");
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

    private static InvestmentCategoryResponseDto ToDto(InvestmentCategory c) => new(
        c.Id,
        c.Name,
        c.Type.HasValue ? InvestmentTypeCatalog.GroupOf[c.Type.Value] : null,
        c.Type,
        c.AppliedAmount,
        c.InterestRate,
        c.MonthlyContribution,
        c.IsActive);
}
