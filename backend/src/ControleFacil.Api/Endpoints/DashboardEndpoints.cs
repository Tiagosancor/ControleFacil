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
    }
}
