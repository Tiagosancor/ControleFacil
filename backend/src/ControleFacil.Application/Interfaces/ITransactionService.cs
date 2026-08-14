using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface ITransactionService
{
    Task<PagedResultDto<TransactionResponseDto>> GetAllAsync(TransactionFilterDto filter, int page, int pageSize);
    Task<TransactionResponseDto> GetByIdAsync(int id);
    Task<IReadOnlyList<TransactionResponseDto>> CreateAsync(TransactionCreateDto dto);
    Task<TransactionResponseDto> UpdateAsync(int id, TransactionUpdateDto dto);
    Task DeleteAsync(int id);
    Task DeleteSeriesAsync(int seriesId);
}
