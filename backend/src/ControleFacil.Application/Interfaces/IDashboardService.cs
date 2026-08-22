using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IDashboardService
{
    Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month);
    Task<IReadOnlyList<DueAlertItemDto>> GetDueSoonAsync();
    Task<MonthlyGoalComparisonDto?> GetGoalComparisonAsync(int year, int month);
    Task<IReadOnlyList<HistoricalMonthDto>> GetHistoricalSummaryAsync(int year, int month, int monthsBack = 3);
    Task<IReadOnlyList<AnalysisDto>> GetAutomaticAnalysesAsync(int year, int month);
}
