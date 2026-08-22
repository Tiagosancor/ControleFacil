using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports").RequireAuthorization();

        group.MapGet("/dre", async (int year, IReportService service) =>
        {
            var result = await service.GetDreAsync(year);
            return Results.Ok(result);
        });

        group.MapGet("/pending", async (IReportService service) =>
        {
            var result = await service.GetPendingAsync();
            return Results.Ok(result);
        });
    }
}
