using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class MonthlyGoalEndpoints
{
    public static void MapMonthlyGoalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/monthly-goals").RequireAuthorization();

        group.MapGet("/", async (
            IMonthlyGoalService service,
            int? year = null,
            int? month = null) =>
        {
            var result = await service.GetAllAsync(year, month);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, IMonthlyGoalService service) =>
        {
            var goal = await service.GetByIdAsync(id);
            return Results.Ok(goal);
        });

        group.MapPost("/", async (
            MonthlyGoalCreateDto dto,
            IValidator<MonthlyGoalCreateDto> validator,
            IMonthlyGoalService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var goal = await service.CreateAsync(dto);
            return Results.Created($"/api/monthly-goals/{goal.Id}", goal);
        });

        group.MapPut("/{id:int}", async (
            int id,
            MonthlyGoalUpdateDto dto,
            IValidator<MonthlyGoalUpdateDto> validator,
            IMonthlyGoalService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var goal = await service.UpdateAsync(id, dto);
            return Results.Ok(goal);
        });

        group.MapDelete("/{id:int}", async (int id, IMonthlyGoalService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
