using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class InvestmentEntryEndpoints
{
    public static void MapInvestmentEntryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/investment-entries").RequireAuthorization();

        group.MapGet("/", async (
            IInvestmentEntryService service,
            int? year = null,
            int? month = null) =>
        {
            var result = await service.GetAllAsync(year, month);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, IInvestmentEntryService service) =>
        {
            var entry = await service.GetByIdAsync(id);
            return Results.Ok(entry);
        });

        group.MapPost("/", async (
            InvestmentEntryCreateDto dto,
            IValidator<InvestmentEntryCreateDto> validator,
            IInvestmentEntryService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var entry = await service.CreateAsync(dto);
            return Results.Created($"/api/investment-entries/{entry.Id}", entry);
        });

        group.MapPut("/{id:int}", async (
            int id,
            InvestmentEntryUpdateDto dto,
            IValidator<InvestmentEntryUpdateDto> validator,
            IInvestmentEntryService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var entry = await service.UpdateAsync(id, dto);
            return Results.Ok(entry);
        });

        group.MapDelete("/{id:int}", async (int id, IInvestmentEntryService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
