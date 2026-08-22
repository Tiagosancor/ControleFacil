using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IDashboardService
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month);
    Task<IReadOnlyList<DueAlertItemDto>> GetDueSoonAsync();
}
