using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;

namespace ControleFacil.Api.Endpoints;

public static class UsageEventEndpoints
{
    public static void MapUsageEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/usage-events").RequireAuthorization();

        // Sem camada de Service, de propósito — é inserção pura, sem lógica nenhuma pra
        // encapsular. Qualquer usuário autenticado pode logar as PRÓPRIAS ações (por
        // isso só RequireAuthorization() do grupo, não AdminOnly).
        group.MapPost("/", async (
            UsageEventCreateDto dto,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser) =>
        {
            var usageEvent = new UsageEvent
            {
                UserId = currentUser.UserId,
                EventType = dto.EventType,
                Metadata = dto.Metadata,
                CreatedAt = DateTime.UtcNow,
            };

            await unitOfWork.UsageEvents.AddAsync(usageEvent);
            await unitOfWork.SaveChangesAsync();

            var response = new UsageEventResponseDto(
                usageEvent.Id, usageEvent.UserId, usageEvent.EventType, usageEvent.Metadata, usageEvent.CreatedAt);

            return Results.Created($"/api/usage-events/{usageEvent.Id}", response);
        });

        // Os dois abaixo são leitura agregada pra tela /admin — têm lógica de verdade
        // (janela de tempo, join, agrupamento), por isso passam por IUsageEventService
        // em vez de tocar IUnitOfWork direto, diferente do POST acima. Gated por
        // AdminOnly (Sprint Admin-1) — não fazem sentido pra um usuário comum ver.
        group.MapGet("/login-history", async (
            IUsageEventService service,
            int? userId,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await service.GetLoginHistoryAsync(userId, page, pageSize);
            return Results.Ok(result);
        }).RequireAuthorization("AdminOnly");

        group.MapGet("/logged-in-users", async (
            IUsageEventService service,
            int? minutes) =>
        {
            var result = await service.GetLoggedInUsersAsync(minutes);
            return Results.Ok(result);
        }).RequireAuthorization("AdminOnly");
    }
}
