using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IMonthlyGoalService
{
    Task<IReadOnlyList<MonthlyGoalResponseDto>> GetAllAsync(int? year, int? month);
    Task<MonthlyGoalResponseDto> GetByIdAsync(int id);
    Task<MonthlyGoalResponseDto> CreateAsync(MonthlyGoalCreateDto dto);
    Task<MonthlyGoalResponseDto> UpdateAsync(int id, MonthlyGoalUpdateDto dto);
    Task DeleteAsync(int id);
}
