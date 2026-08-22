using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface ICategoryBudgetService
{
    Task<IReadOnlyList<CategoryBudgetResponseDto>> GetAllAsync(int? year, int? month);
    Task<CategoryBudgetResponseDto> GetByIdAsync(int id);
    Task<CategoryBudgetResponseDto> CreateAsync(CategoryBudgetCreateDto dto);
    Task<CategoryBudgetResponseDto> UpdateAsync(int id, CategoryBudgetUpdateDto dto);
    Task DeleteAsync(int id);
}
