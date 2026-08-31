using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    Task SendDueAlertEmailAsync(string toEmail, string userName, IReadOnlyList<DueAlertItemDto> items);
    Task SendContactMessageEmailAsync(string name, string email, string message);
}
