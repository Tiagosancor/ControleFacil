using System.Security.Claims;
using ControleFacil.Api.Extensions;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;

namespace ControleFacil.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterDto dto,
            IValidator<RegisterDto> validator,
            IAuthService authService) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var user = await authService.RegisterAsync(dto);
            return Results.Created($"/api/users/{user.Id}", user);
        });

        group.MapPost("/login", async (
            LoginDto dto,
            IValidator<LoginDto> validator,
            IAuthService authService) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            var result = await authService.LoginAsync(dto);
            return Results.Ok(result);
        }).RequireRateLimiting("login");

        group.MapGet("/me", async (ClaimsPrincipal principal, IUnitOfWork unitOfWork) =>
        {
            var user = await unitOfWork.Users.GetByIdAsync(principal.GetUserId());
            if (user is null) return Results.NotFound();

            return Results.Ok(new UserResponseDto(user.Id, user.Name, user.Email, user.Role));
        }).RequireAuthorization();

        group.MapPost("/forgot-password", async (
            ForgotPasswordDto dto,
            IValidator<ForgotPasswordDto> validator,
            IAuthService authService) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            await authService.ForgotPasswordAsync(dto);
            return Results.Ok(new { message = "Se o e-mail informado estiver cadastrado, você receberá um link de recuperação em instantes." });
        }).RequireRateLimiting("forgot-password");

        group.MapPost("/reset-password", async (
            ResetPasswordDto dto,
            IValidator<ResetPasswordDto> validator,
            IAuthService authService) =>
        {
            var problem = await validator.ValidateOrProblemAsync(dto);
            if (problem != null) return problem;

            await authService.ResetPasswordAsync(dto);
            return Results.Ok(new { message = "Senha redefinida com sucesso." });
        });
    }
}
