using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IBankAccountService
{
    Task<PagedResultDto<BankAccountResponseDto>> GetAllAsync(bool includeInactive, int page, int pageSize);
    Task<BankAccountResponseDto> GetByIdAsync(int id);
    Task<BankAccountResponseDto> CreateAsync(BankAccountCreateDto dto);
    Task<BankAccountResponseDto> UpdateAsync(int id, BankAccountUpdateDto dto);
    Task DeleteAsync(int id);
}
