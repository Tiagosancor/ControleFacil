using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Enums;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/transactions").RequireAuthorization();

        group.MapGet("/", async (
            ITransactionService service,
            int? categoryId = null,
            int? bankAccountId = null,
            TransactionStatus? status = null,
            int? year = null,
            int? month = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var filter = new TransactionFilterDto(categoryId, bankAccountId, status, year, month, startDate, endDate);
            var result = await service.GetAllAsync(filter, page, pageSize);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, ITransactionService service) =>
        {
            var transaction = await service.GetByIdAsync(id);
            return Results.Ok(transaction);
        });

        group.MapPost("/", async (
            TransactionCreateDto dto,
            IValidator<TransactionCreateDto> validator,
            ITransactionService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/transactions/{created[0].Id}", created);
        });

        group.MapPut("/{id:int}", async (
            int id,
            TransactionUpdateDto dto,
            IValidator<TransactionUpdateDto> validator,
            ITransactionService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var transaction = await service.UpdateAsync(id, dto);
            return Results.Ok(transaction);
        });

        group.MapDelete("/{id:int}", async (int id, ITransactionService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });

        group.MapDelete("/series/{seriesId:int}", async (int seriesId, ITransactionService service) =>
        {
            await service.DeleteSeriesAsync(seriesId);
            return Results.NoContent();
        });
    }
}
