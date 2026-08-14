using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Application.Services;
using ControleFacil.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ControleFacil.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();

        return services;
    }
}
