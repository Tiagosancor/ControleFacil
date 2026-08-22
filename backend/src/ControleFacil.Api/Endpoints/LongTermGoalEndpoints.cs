using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class LongTermGoalEndpoints
{
    public static void MapLongTermGoalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/long-term-goals").RequireAuthorization();

        group.MapGet("/", async (ILongTermGoalService service) =>
        {
            var result = await service.GetAllAsync();
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, ILongTermGoalService service) =>
        {
            var goal = await service.GetByIdAsync(id);
            return Results.Ok(goal);
        });

        group.MapPost("/", async (
            LongTermGoalCreateDto dto,
            IValidator<LongTermGoalCreateDto> validator,
            ILongTermGoalService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var goal = await service.CreateAsync(dto);
            return Results.Created($"/api/long-term-goals/{goal.Id}", goal);
        });

        group.MapPut("/{id:int}", async (
            int id,
            LongTermGoalUpdateDto dto,
            IValidator<LongTermGoalUpdateDto> validator,
            ILongTermGoalService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var goal = await service.UpdateAsync(id, dto);
            return Results.Ok(goal);
        });

        group.MapDelete("/{id:int}", async (int id, ILongTermGoalService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
