using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class CreditCardEndpoints
{
    public static void MapCreditCardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/credit-cards").RequireAuthorization();

        group.MapGet("/", async (
            ICreditCardService service,
            bool includeInactive = false) =>
        {
            var result = await service.GetAllAsync(includeInactive);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, ICreditCardService service) =>
        {
            var card = await service.GetByIdAsync(id);
            return Results.Ok(card);
        });

        group.MapGet("/{id:int}/invoice", async (int id, int year, int month, ICreditCardService service) =>
        {
            var invoice = await service.GetInvoiceAsync(id, year, month);
            return Results.Ok(invoice);
        });

        group.MapPost("/", async (
            CreditCardCreateDto dto,
            IValidator<CreditCardCreateDto> validator,
            ICreditCardService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var card = await service.CreateAsync(dto);
            return Results.Created($"/api/credit-cards/{card.Id}", card);
        });

        group.MapPut("/{id:int}", async (
            int id,
            CreditCardUpdateDto dto,
            IValidator<CreditCardUpdateDto> validator,
            ICreditCardService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var card = await service.UpdateAsync(id, dto);
            return Results.Ok(card);
        });

        group.MapDelete("/{id:int}", async (int id, ICreditCardService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
