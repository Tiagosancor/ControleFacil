using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;

namespace ControleFacil.Api.Endpoints;

// Esqueleto só pra inserção manual de teste — sem camada de Service, de propósito
// (a lógica de agregação/relatórios fica pra depois, feita manualmente). Diferente de
// todo outro endpoint do projeto, que sempre passa por um I*Service.
public static class UsageEventEndpoints
{
    public static void MapUsageEventEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/usage-events").RequireAuthorization();

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
    }
}
