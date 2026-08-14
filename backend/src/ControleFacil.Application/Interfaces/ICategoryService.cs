using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface ICategoryService
{
    Task<PagedResultDto<CategoryResponseDto>> GetAllAsync(bool includeInactive, int page, int pageSize);
    Task<CategoryResponseDto> GetByIdAsync(int id);
    Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto);
    Task<CategoryResponseDto> UpdateAsync(int id, CategoryUpdateDto dto);
    Task DeleteAsync(int id);
}
