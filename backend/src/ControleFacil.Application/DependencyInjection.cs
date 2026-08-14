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
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<ITransactionService, TransactionService>();

        services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        services.AddScoped<IValidator<CategoryCreateDto>, CategoryCreateDtoValidator>();
        services.AddScoped<IValidator<CategoryUpdateDto>, CategoryUpdateDtoValidator>();
        services.AddScoped<IValidator<BankAccountCreateDto>, BankAccountCreateDtoValidator>();
        services.AddScoped<IValidator<BankAccountUpdateDto>, BankAccountUpdateDtoValidator>();
        services.AddScoped<IValidator<TransactionCreateDto>, TransactionCreateDtoValidator>();
        services.AddScoped<IValidator<TransactionUpdateDto>, TransactionUpdateDtoValidator>();

        return services;
    }
}
