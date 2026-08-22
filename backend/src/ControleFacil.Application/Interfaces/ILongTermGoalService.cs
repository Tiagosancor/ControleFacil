using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface ILongTermGoalService
{
    Task<IReadOnlyList<LongTermGoalResponseDto>> GetAllAsync();
    Task<LongTermGoalResponseDto> GetByIdAsync(int id);
    Task<LongTermGoalResponseDto> CreateAsync(LongTermGoalCreateDto dto);
    Task<LongTermGoalResponseDto> UpdateAsync(int id, LongTermGoalUpdateDto dto);
    Task DeleteAsync(int id);
}
