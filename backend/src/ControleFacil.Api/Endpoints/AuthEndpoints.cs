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

            return Results.Ok(new UserResponseDto(user.Id, user.Name, user.Email));
        }).RequireAuthorization();
    }
}
