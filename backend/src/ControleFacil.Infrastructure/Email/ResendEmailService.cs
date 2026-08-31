using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ControleFacil.Infrastructure.Email;

public class ResendEmailService : IEmailService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly HttpClient _httpClient;
    private readonly ResendOptions _options;

    public ResendEmailService(HttpClient httpClient, IOptions<ResendOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        await SendAsync(toEmail, "Recuperação de senha - Semeia Grana", $"""
            <p>Você solicitou a recuperação de senha da sua conta Semeia Grana.</p>
            <p><a href="{resetLink}">Clique aqui para redefinir sua senha</a></p>
            <p>Este link expira em 45 minutos. Se você não solicitou essa recuperação, ignore este e-mail.</p>
            """);
    }

    public async Task SendDueAlertEmailAsync(string toEmail, string userName, IReadOnlyList<DueAlertItemDto> items)
    {
        var rows = new StringBuilder();
        foreach (var item in items)
        {
            var amount = item.Amount.ToString("C", PtBr);
            var dueDate = item.EntryDate.ToString("dd/MM/yyyy");
            var statusLabel = item.IsOverdue ? "vencido" : $"vence em {dueDate}";
            rows.Append($"<li>{System.Net.WebUtility.HtmlEncode(item.Description)} — {amount} ({statusLabel})</li>");
        }

        await SendAsync(toEmail, "Lançamentos pendentes - Semeia Grana", $"""
            <p>Olá, {System.Net.WebUtility.HtmlEncode(userName)}.</p>
            <p>Você tem {items.Count} lançamento(s) pendente(s) vencendo em breve ou já vencido(s):</p>
            <ul>{rows}</ul>
            <p>Acesse o Semeia Grana pra marcar como pago ou revisar os detalhes.</p>
            """);
    }

    public async Task SendContactMessageEmailAsync(string name, string email, string message)
    {
        var encodedMessage = System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br>");
        await SendAsync(_options.ContactRecipientEmail, "Nova mensagem de contato - Semeia Grana", $"""
            <p><strong>Nome:</strong> {System.Net.WebUtility.HtmlEncode(name)}</p>
            <p><strong>E-mail:</strong> {System.Net.WebUtility.HtmlEncode(email)}</p>
            <p><strong>Mensagem:</strong></p>
            <p>{encodedMessage}</p>
            """, replyTo: email);
    }

    private async Task SendAsync(string toEmail, string subject, string html, string? replyTo = null)
    {
        var payload = new
        {
            from = $"{_options.FromName} <{_options.FromEmail}>",
            to = new[] { toEmail },
            subject,
            html,
            reply_to = replyTo,
        };

        var response = await _httpClient.PostAsJsonAsync("emails", payload);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Falha ao enviar e-mail via Resend ({(int)response.StatusCode}): {body}");
        }
    }
}
