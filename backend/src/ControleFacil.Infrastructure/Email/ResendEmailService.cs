using System.Net.Http.Json;
using ControleFacil.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ControleFacil.Infrastructure.Email;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ResendOptions _options;

    public ResendEmailService(HttpClient httpClient, IOptions<ResendOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var payload = new
        {
            from = $"{_options.FromName} <{_options.FromEmail}>",
            to = new[] { toEmail },
            subject = "Recuperação de senha - Semeia Grana",
            html = $"""
                <p>Você solicitou a recuperação de senha da sua conta Semeia Grana.</p>
                <p><a href="{resetLink}">Clique aqui para redefinir sua senha</a></p>
                <p>Este link expira em 45 minutos. Se você não solicitou essa recuperação, ignore este e-mail.</p>
                """,
        };

        var response = await _httpClient.PostAsJsonAsync("emails", payload);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Falha ao enviar e-mail via Resend ({(int)response.StatusCode}): {body}");
        }
    }
}
