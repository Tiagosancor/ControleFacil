using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Endpoints;

public static class BankEndpoints
{
    public static void MapBankEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/banks").RequireAuthorization();

        group.MapGet("/", async (string? search, IBankService bankService) =>
        {
            var results = await bankService.SearchAsync(search);
            return Results.Ok(results);
        });

        // A sincronização roda sozinha (BankSyncBackgroundService, semanal) — esse endpoint
        // só existe pra forçar uma atualização manual sem reiniciar a API (ex.: logo após o
        // primeiro deploy, ou pra depuração).
        group.MapPost("/sync", async (IBankSyncService bankSyncService) =>
        {
            var count = await bankSyncService.SyncAsync();
            return Results.Ok(new { synced = count });
        }).RequireAuthorization("AdminOnly");
    }
}
