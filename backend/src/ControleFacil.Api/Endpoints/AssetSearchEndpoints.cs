using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Endpoints;

public static class AssetSearchEndpoints
{
    public static void MapAssetSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/investments").RequireAuthorization();

        // Sem Service de Application no meio, de propósito — IAssetSearchService já
        // encapsula toda a lógica (cache + chamada à brapi.dev), mesmo padrão de
        // "porta de infraestrutura injetada direto" já usado com IEmailService.
        group.MapGet("/search-assets", async (
            string type,
            string search,
            IAssetSearchService assetSearchService) =>
        {
            var results = await assetSearchService.SearchAsync(type, search);
            return Results.Ok(results);
        });
    }
}
