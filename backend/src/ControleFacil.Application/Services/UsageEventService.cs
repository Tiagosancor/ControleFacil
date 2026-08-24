using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ControleFacil.Application.Services;

public class UsageEventService : IUsageEventService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public UsageEventService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    // Visão de admin: todos os usuários juntos, evento Login, mais recente primeiro.
    // userId é um filtro opcional, não um requisito — a tela nasce mostrando tudo.
    public async Task<PagedResultDto<LoginHistoryItemDto>> GetLoginHistoryAsync(int? userId, int page, int pageSize)
    {
        var query = _unitOfWork.UsageEvents.Query().Where(e => e.EventType == UsageEventType.Login);
        if (userId.HasValue)
            query = query.Where(e => e.UserId == userId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_unitOfWork.Users.Query(), e => e.UserId, u => u.Id,
                (e, u) => new LoginHistoryItemDto(u.Id, u.Name, u.Email, e.CreatedAt))
            .ToListAsync();

        return new PagedResultDto<LoginHistoryItemDto>(total, page, pageSize, items);
    }

    // "Logado agora" é uma aproximação, não um fato — o JWT é stateless, não existe
    // sessão ativa rastreada no servidor. Proxy: teve um evento Login dentro da janela
    // (default = mesmo tempo de expiração do token, pra não afirmar "online" além do
    // tempo em que o token da pessoa ainda é válido).
    public async Task<IReadOnlyList<LoggedInUserDto>> GetLoggedInUsersAsync(int? minutes)
    {
        var windowMinutes = minutes ?? double.Parse(_configuration["Jwt:ExpireMinutes"] ?? "120");
        var since = DateTime.UtcNow.AddMinutes(-windowMinutes);

        var recentLogins = await _unitOfWork.UsageEvents.Query()
            .Where(e => e.EventType == UsageEventType.Login && e.CreatedAt >= since)
            .Select(e => new { e.UserId, e.CreatedAt })
            .ToListAsync();

        if (recentLogins.Count == 0)
            return Array.Empty<LoggedInUserDto>();

        var latestPerUser = recentLogins
            .GroupBy(e => e.UserId)
            .Select(g => new { UserId = g.Key, LastLoginAt = g.Max(e => e.CreatedAt) })
            .ToList();

        var userIds = latestPerUser.Select(x => x.UserId).ToList();
        var users = await _unitOfWork.Users.Query()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        return latestPerUser
            .Where(l => users.ContainsKey(l.UserId))
            .Select(l => new LoggedInUserDto(l.UserId, users[l.UserId].Name, users[l.UserId].Email, l.LastLoginAt))
            .OrderByDescending(x => x.LastLoginAt)
            .ToList();
    }
}
