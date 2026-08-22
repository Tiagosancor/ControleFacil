namespace ControleFacil.Application.Interfaces;

public interface IDueAlertService
{
    /// <summary>
    /// Varre lançamentos pendentes com vencimento dentro da janela configurada (ou já
    /// vencidos) que ainda não foram avisados, agrupa por usuário e envia um e-mail por
    /// usuário via <see cref="IEmailService"/>. Marca cada lançamento avisado pra nunca
    /// reenviar. Retorna quantos e-mails foram enviados.
    /// </summary>
    Task<int> SendPendingDueAlertsAsync(CancellationToken cancellationToken = default);
}
