using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ControleFacil.Infrastructure.Banks;

public class BrasilApiBankClient : IBrasilApiBankClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;
    private readonly ILogger<BrasilApiBankClient> _logger;

    public BrasilApiBankClient(HttpClient httpClient, ILogger<BrasilApiBankClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BankSyncItemDto>> FetchAllAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/banks/v1");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BrasilAPI retornou {StatusCode} ao buscar a lista de bancos", response.StatusCode);
                return Array.Empty<BankSyncItemDto>();
            }

            var raw = await response.Content.ReadFromJsonAsync<List<RawBank>>(JsonOptions) ?? new List<RawBank>();
            return raw
                .Where(r => !string.IsNullOrWhiteSpace(r.Ispb) && !string.IsNullOrWhiteSpace(r.Name))
                .Select(r => new BankSyncItemDto(r.Ispb, r.Code, r.Name, r.FullName, r.LogoUrl))
                .ToList();
        }
        catch (Exception ex)
        {
            // A sincronização roda em background e nunca pode derrubar a aplicação — se a
            // BrasilAPI estiver fora do ar, GET /api/banks continua servindo o que já está
            // salvo localmente da última sincronização bem-sucedida.
            _logger.LogWarning(ex, "Falha ao buscar lista de bancos na BrasilAPI");
            return Array.Empty<BankSyncItemDto>();
        }
    }

    private class RawBank
    {
        public string Ispb { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? Code { get; set; }
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("logo_url")]
        public string? LogoUrl { get; set; }
    }
}
