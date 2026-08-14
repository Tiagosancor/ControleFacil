using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories").RequireAuthorization();

        group.MapGet("/", async (
            ICategoryService service,
            bool includeInactive = false,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await service.GetAllAsync(includeInactive, page, pageSize);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, ICategoryService service) =>
        {
            var category = await service.GetByIdAsync(id);
            return Results.Ok(category);
        });

        group.MapPost("/", async (
            CategoryCreateDto dto,
            IValidator<CategoryCreateDto> validator,
            ICategoryService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var category = await service.CreateAsync(dto);
            return Results.Created($"/api/categories/{category.Id}", category);
        });

        group.MapPut("/{id:int}", async (
            int id,
            CategoryUpdateDto dto,
            IValidator<CategoryUpdateDto> validator,
            ICategoryService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var category = await service.UpdateAsync(id, dto);
            return Results.Ok(category);
        });

        group.MapDelete("/{id:int}", async (int id, ICategoryService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
