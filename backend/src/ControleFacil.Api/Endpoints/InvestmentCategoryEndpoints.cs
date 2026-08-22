using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class InvestmentCategoryEndpoints
{
    public static void MapInvestmentCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/investment-categories").RequireAuthorization();

        group.MapGet("/", async (
            IInvestmentCategoryService service,
            bool includeInactive = false) =>
        {
            var result = await service.GetAllAsync(includeInactive);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, IInvestmentCategoryService service) =>
        {
            var category = await service.GetByIdAsync(id);
            return Results.Ok(category);
        });

        group.MapPost("/", async (
            InvestmentCategoryCreateDto dto,
            IValidator<InvestmentCategoryCreateDto> validator,
            IInvestmentCategoryService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var category = await service.CreateAsync(dto);
            return Results.Created($"/api/investment-categories/{category.Id}", category);
        });

        group.MapPut("/{id:int}", async (
            int id,
            InvestmentCategoryUpdateDto dto,
            IValidator<InvestmentCategoryUpdateDto> validator,
            IInvestmentCategoryService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var category = await service.UpdateAsync(id, dto);
            return Results.Ok(category);
        });

        group.MapDelete("/{id:int}", async (int id, IInvestmentCategoryService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
