using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Services;

// Roda a checagem de alertas de vencimento assim que a app sobe (importante: é
// exatamente o momento em que a Render acorda o serviço no free tier após dormir) e
// depois a cada CheckInterval enquanto o processo continuar de pé. Não há garantia de
// cadência exata (a app pode dormir de novo entre checagens), mas Transaction.DueAlertSentAt
// garante que nenhum lançamento seja avisado mais de uma vez, então uma checagem
// "atrasada" só significa um alerta um pouco mais tardio, nunca duplicado.
public class DueAlertBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);

    private readonly IServiceProvider _services;
    private readonly ILogger<DueAlertBackgroundService> _logger;

    public DueAlertBackgroundService(IServiceProvider services, ILogger<DueAlertBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);
        do
        {
            await CheckAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var dueAlertService = scope.ServiceProvider.GetRequiredService<IDueAlertService>();
            var sent = await dueAlertService.SendPendingDueAlertsAsync(stoppingToken);
            if (sent > 0)
                _logger.LogInformation("Alertas de vencimento: {Count} e-mail(s) enviado(s).", sent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao processar alertas de vencimento.");
        }
    }
}
