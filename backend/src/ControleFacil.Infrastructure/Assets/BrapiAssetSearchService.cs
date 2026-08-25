using System.Net.Http.Json;
using System.Text.Json;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ControleFacil.Infrastructure.Assets;

public class BrapiAssetSearchService : IAssetSearchService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BrapiAssetSearchService> _logger;

    public BrapiAssetSearchService(HttpClient httpClient, IMemoryCache cache, ILogger<BrapiAssetSearchService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AssetSearchResultDto>> SearchAsync(string type, string search)
    {
        if (string.IsNullOrWhiteSpace(search) || search.Trim().Length < 2)
            return Array.Empty<AssetSearchResultDto>();

        // Cache local reduz o consumo da cota mensal gratuita da brapi.dev (15 mil
        // requisições/mês) — tickers não mudam de minuto a minuto, então 10min é seguro.
        var cacheKey = $"asset-search:{type}:{search.Trim().ToLowerInvariant()}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<AssetSearchResultDto>? cached) && cached is not null)
            return cached;

        try
        {
            var response = await _httpClient.GetAsync(
                $"api/v2/tickers?search={Uri.EscapeDataString(search)}&type={Uri.EscapeDataString(type)}&limit=10");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("brapi.dev retornou {StatusCode} ao buscar ativos", response.StatusCode);
                return Array.Empty<AssetSearchResultDto>();
            }

            var payload = await response.Content.ReadFromJsonAsync<BrapiTickersResponse>(JsonOptions);
            var results = (payload?.Results ?? new List<BrapiTickerResult>())
                .Where(r => !string.IsNullOrWhiteSpace(r.Symbol))
                .Select(r => new AssetSearchResultDto(r.Symbol, r.Name))
                .ToList();

            _cache.Set(cacheKey, (IReadOnlyList<AssetSearchResultDto>)results, CacheDuration);
            return results;
        }
        catch (Exception ex)
        {
            // Nunca deixa a busca de sugestões derrubar o formulário — o campo de nome
            // sempre permite digitar manualmente como fallback (AssetAutocomplete no
            // frontend trata lista vazia como "sem sugestão", não como erro).
            _logger.LogWarning(ex, "Falha ao consultar brapi.dev pra busca de ativos");
            return Array.Empty<AssetSearchResultDto>();
        }
    }

    private class BrapiTickersResponse
    {
        public List<BrapiTickerResult> Results { get; set; } = new();
    }

    private class BrapiTickerResult
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
