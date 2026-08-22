using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;

namespace ControleFacil.Api.Tests.TestHelpers;

public class FakeEmailService : IEmailService
{
    public List<(string ToEmail, string ResetLink)> PasswordResetCalls { get; } = new();
    public List<(string ToEmail, string UserName, IReadOnlyList<DueAlertItemDto> Items)> DueAlertCalls { get; } = new();

    public bool ThrowOnDueAlert { get; set; }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        PasswordResetCalls.Add((toEmail, resetLink));
        return Task.CompletedTask;
    }

    public Task SendDueAlertEmailAsync(string toEmail, string userName, IReadOnlyList<DueAlertItemDto> items)
    {
        if (ThrowOnDueAlert)
            throw new InvalidOperationException("Falha simulada de envio.");

        DueAlertCalls.Add((toEmail, userName, items));
        return Task.CompletedTask;
    }
}
