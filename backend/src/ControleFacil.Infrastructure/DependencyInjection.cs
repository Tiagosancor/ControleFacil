using System.Net.Http.Headers;
using ControleFacil.Application;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Auth;
using ControleFacil.Infrastructure.Data;
using ControleFacil.Infrastructure.Email;
using ControleFacil.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ControleFacil.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionSeriesRepository, TransactionSeriesRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ICategoryBudgetRepository, CategoryBudgetRepository>();
        services.AddScoped<IMonthlyGoalRepository, MonthlyGoalRepository>();
        services.AddScoped<IInvestmentCategoryRepository, InvestmentCategoryRepository>();
        services.AddScoped<IInvestmentEntryRepository, InvestmentEntryRepository>();
        services.AddScoped<ILongTermGoalRepository, LongTermGoalRepository>();
        services.AddScoped<ICreditCardRepository, CreditCardRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.Configure<ResendOptions>(configuration.GetSection("Resend"));
        services.Configure<DueAlertOptions>(configuration.GetSection("DueAlerts"));
        services.AddHttpClient<IEmailService, ResendEmailService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ResendOptions>>().Value;
            client.BaseAddress = new Uri("https://api.resend.com/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        });

        return services;
    }
}
