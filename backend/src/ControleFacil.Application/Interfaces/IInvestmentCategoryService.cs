using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IInvestmentCategoryService
{
    Task<IReadOnlyList<InvestmentCategoryResponseDto>> GetAllAsync(bool includeInactive);
    Task<InvestmentCategoryResponseDto> GetByIdAsync(int id);
    Task<InvestmentCategoryResponseDto> CreateAsync(InvestmentCategoryCreateDto dto);
    Task<InvestmentCategoryResponseDto> UpdateAsync(int id, InvestmentCategoryUpdateDto dto);
    Task DeleteAsync(int id);
}
