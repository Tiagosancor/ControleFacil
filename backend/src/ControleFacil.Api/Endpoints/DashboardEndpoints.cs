using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dashboard").RequireAuthorization();

        group.MapGet("/summary", async (int year, int month, IDashboardService service) =>
        {
            var summary = await service.GetMonthlySummaryAsync(year, month);
            return Results.Ok(summary);
        });

        group.MapGet("/due-soon", async (IDashboardService service) =>
        {
            var items = await service.GetDueSoonAsync();
            return Results.Ok(items);
        });

        group.MapGet("/goal-comparison", async (int year, int month, IDashboardService service) =>
        {
            var comparison = await service.GetGoalComparisonAsync(year, month);
            return Results.Ok(comparison);
        });

        group.MapGet("/historical", async (int year, int month, IDashboardService service, int monthsBack = 3) =>
        {
            var result = await service.GetHistoricalSummaryAsync(year, month, monthsBack);
            return Results.Ok(result);
        });

        group.MapGet("/analyses", async (int year, int month, IDashboardService service) =>
        {
            var result = await service.GetAutomaticAnalysesAsync(year, month);
            return Results.Ok(result);
        });
    }
}
