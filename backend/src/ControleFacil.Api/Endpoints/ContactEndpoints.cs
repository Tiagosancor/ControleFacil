using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using FluentValidation;

namespace ControleFacil.Api.Endpoints;

public static class ContactEndpoints
{
    public static void MapContactEndpoints(this WebApplication app)
    {
        app.MapPost("/api/contact", async (
            ContactDto dto,
            IValidator<ContactDto> validator,
            IEmailService emailService) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            await emailService.SendContactMessageEmailAsync(dto.Name, dto.Email, dto.Message);
            return Results.Ok(new { message = "Mensagem enviada com sucesso." });
        }).RequireRateLimiting("contact");
    }
}
