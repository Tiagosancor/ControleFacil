using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControleFacil.Application.Services;

public class DueAlertService : IDueAlertService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly DueAlertOptions _options;
    private readonly ILogger<DueAlertService> _logger;

    public DueAlertService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IOptions<DueAlertOptions> options,
        ILogger<DueAlertService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int> SendPendingDueAlertsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(_options.DaysBefore);

        // Sem filtro de UserId de propósito: esse job cruza todos os usuários, diferente
        // dos services normais (escopados via ICurrentUserService, que não existe fora de
        // uma requisição HTTP).
        var dueTransactions = await _unitOfWork.Transactions.Query()
            .Include(t => t.User)
            .Where(t =>
                t.Status == TransactionStatus.Pending &&
                t.DueAlertSentAt == null &&
                t.EntryDate <= threshold)
            .ToListAsync(cancellationToken);

        if (dueTransactions.Count == 0)
            return 0;

        var emailsSent = 0;
        foreach (var group in dueTransactions.GroupBy(t => t.UserId))
        {
            var user = group.First().User;
            if (user is null)
                continue;

            var items = group
                .OrderBy(t => t.EntryDate)
                .Select(t => new DueAlertItemDto(t.Id, t.Description, t.Amount, t.EntryDate, t.EntryDate < today))
                .ToList();

            try
            {
                await _emailService.SendDueAlertEmailAsync(user.Email, user.Name, items);
            }
            catch (Exception ex)
            {
                // Não marca DueAlertSentAt: se o envio falhar, tenta de novo no próximo
                // ciclo em vez de perder o alerta silenciosamente.
                _logger.LogError(ex, "Falha ao enviar alerta de vencimento pro usuário {UserId}.", user.Id);
                continue;
            }

            var now = DateTime.UtcNow;
            foreach (var transaction in group)
                transaction.DueAlertSentAt = now;

            emailsSent++;
        }

        await _unitOfWork.SaveChangesAsync();
        return emailsSent;
    }
}
