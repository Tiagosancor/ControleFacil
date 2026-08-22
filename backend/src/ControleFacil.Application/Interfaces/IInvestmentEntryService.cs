using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IInvestmentEntryService
{
    Task<IReadOnlyList<InvestmentEntryResponseDto>> GetAllAsync(int? year, int? month);
    Task<InvestmentEntryResponseDto> GetByIdAsync(int id);
    Task<InvestmentEntryResponseDto> CreateAsync(InvestmentEntryCreateDto dto);
    Task<InvestmentEntryResponseDto> UpdateAsync(int id, InvestmentEntryUpdateDto dto);
    Task DeleteAsync(int id);
}
