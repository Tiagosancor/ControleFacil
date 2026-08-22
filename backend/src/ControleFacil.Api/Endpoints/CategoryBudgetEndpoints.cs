using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class CategoryBudgetEndpoints
{
    public static void MapCategoryBudgetEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/category-budgets").RequireAuthorization();

        group.MapGet("/", async (
            ICategoryBudgetService service,
            int? year = null,
            int? month = null) =>
        {
            var result = await service.GetAllAsync(year, month);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, ICategoryBudgetService service) =>
        {
            var budget = await service.GetByIdAsync(id);
            return Results.Ok(budget);
        });

        group.MapPost("/", async (
            CategoryBudgetCreateDto dto,
            IValidator<CategoryBudgetCreateDto> validator,
            ICategoryBudgetService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var budget = await service.CreateAsync(dto);
            return Results.Created($"/api/category-budgets/{budget.Id}", budget);
        });

        group.MapPut("/{id:int}", async (
            int id,
            CategoryBudgetUpdateDto dto,
            IValidator<CategoryBudgetUpdateDto> validator,
            ICategoryBudgetService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var budget = await service.UpdateAsync(id, dto);
            return Results.Ok(budget);
        });

        group.MapDelete("/{id:int}", async (int id, ICategoryBudgetService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
