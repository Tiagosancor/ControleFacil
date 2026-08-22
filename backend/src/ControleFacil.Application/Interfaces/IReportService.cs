using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IReportService
{
    Task<DreReportDto> GetDreAsync(int year);
    Task<PendingReportDto> GetPendingAsync();
}
