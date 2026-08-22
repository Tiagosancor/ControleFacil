using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface ICreditCardService
{
    Task<IReadOnlyList<CreditCardResponseDto>> GetAllAsync(bool includeInactive);
    Task<CreditCardResponseDto> GetByIdAsync(int id);
    Task<CreditCardResponseDto> CreateAsync(CreditCardCreateDto dto);
    Task<CreditCardResponseDto> UpdateAsync(int id, CreditCardUpdateDto dto);
    Task DeleteAsync(int id);
    Task<CreditCardInvoiceDto> GetInvoiceAsync(int id, int year, int month);
}
