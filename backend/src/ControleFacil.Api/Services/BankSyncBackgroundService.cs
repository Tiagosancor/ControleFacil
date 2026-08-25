using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Services;

// Sincroniza a tabela local Banks com a BrasilAPI assim que a app sobe e depois a cada
// SyncInterval. Se a BrasilAPI estiver fora do ar numa execução, SyncAsync devolve 0 sem
// lançar (ver BrasilApiBankClient) — a tabela local simplesmente fica com os dados da
// última sincronização bem-sucedida, e o job tenta de novo no próximo ciclo.
public class BankSyncBackgroundService : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(7);

    private readonly IServiceProvider _services;
    private readonly ILogger<BankSyncBackgroundService> _logger;

    public BankSyncBackgroundService(IServiceProvider services, ILogger<BankSyncBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SyncInterval);
        do
        {
            await SyncAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SyncAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var bankSyncService = scope.ServiceProvider.GetRequiredService<IBankSyncService>();
            var count = await bankSyncService.SyncAsync(stoppingToken);
            _logger.LogInformation("Sincronização de bancos concluída: {Count} banco(s) processado(s).", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao sincronizar lista de bancos com a BrasilAPI.");
        }
    }
}
