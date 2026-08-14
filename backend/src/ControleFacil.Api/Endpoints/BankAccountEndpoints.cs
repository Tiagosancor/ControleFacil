using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class BankAccountEndpoints
{
    public static void MapBankAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/bank-accounts").RequireAuthorization();

        group.MapGet("/", async (
            IBankAccountService service,
            bool includeInactive = false,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await service.GetAllAsync(includeInactive, page, pageSize);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (int id, IBankAccountService service) =>
        {
            var bankAccount = await service.GetByIdAsync(id);
            return Results.Ok(bankAccount);
        });

        group.MapPost("/", async (
            BankAccountCreateDto dto,
            IValidator<BankAccountCreateDto> validator,
            IBankAccountService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var bankAccount = await service.CreateAsync(dto);
            return Results.Created($"/api/bank-accounts/{bankAccount.Id}", bankAccount);
        });

        group.MapPut("/{id:int}", async (
            int id,
            BankAccountUpdateDto dto,
            IValidator<BankAccountUpdateDto> validator,
            IBankAccountService service) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var bankAccount = await service.UpdateAsync(id, dto);
            return Results.Ok(bankAccount);
        });

        group.MapDelete("/{id:int}", async (int id, IBankAccountService service) =>
        {
            await service.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
