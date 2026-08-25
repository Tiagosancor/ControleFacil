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
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IDueAlertService, DueAlertService>();
        services.AddScoped<ICategoryBudgetService, CategoryBudgetService>();
        services.AddScoped<IMonthlyGoalService, MonthlyGoalService>();
        services.AddScoped<IInvestmentCategoryService, InvestmentCategoryService>();
        services.AddScoped<IInvestmentEntryService, InvestmentEntryService>();
        services.AddScoped<ILongTermGoalService, LongTermGoalService>();
        services.AddScoped<ICreditCardService, CreditCardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUsageEventService, UsageEventService>();
        services.AddScoped<IBankService, BankService>();
        services.AddScoped<IBankSyncService, BankSyncService>();

        services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
        services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
        services.AddScoped<IValidator<ForgotPasswordDto>, ForgotPasswordDtoValidator>();
        services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordDtoValidator>();
        services.AddScoped<IValidator<CategoryCreateDto>, CategoryCreateDtoValidator>();
        services.AddScoped<IValidator<CategoryUpdateDto>, CategoryUpdateDtoValidator>();
        services.AddScoped<IValidator<BankAccountCreateDto>, BankAccountCreateDtoValidator>();
        services.AddScoped<IValidator<BankAccountUpdateDto>, BankAccountUpdateDtoValidator>();
        services.AddScoped<IValidator<TransactionCreateDto>, TransactionCreateDtoValidator>();
        services.AddScoped<IValidator<TransactionUpdateDto>, TransactionUpdateDtoValidator>();
        services.AddScoped<IValidator<CategoryBudgetCreateDto>, CategoryBudgetCreateDtoValidator>();
        services.AddScoped<IValidator<CategoryBudgetUpdateDto>, CategoryBudgetUpdateDtoValidator>();
        services.AddScoped<IValidator<MonthlyGoalCreateDto>, MonthlyGoalCreateDtoValidator>();
        services.AddScoped<IValidator<MonthlyGoalUpdateDto>, MonthlyGoalUpdateDtoValidator>();
        services.AddScoped<IValidator<InvestmentCategoryCreateDto>, InvestmentCategoryCreateDtoValidator>();
        services.AddScoped<IValidator<InvestmentCategoryUpdateDto>, InvestmentCategoryUpdateDtoValidator>();
        services.AddScoped<IValidator<InvestmentEntryCreateDto>, InvestmentEntryCreateDtoValidator>();
        services.AddScoped<IValidator<InvestmentEntryUpdateDto>, InvestmentEntryUpdateDtoValidator>();
        services.AddScoped<IValidator<LongTermGoalCreateDto>, LongTermGoalCreateDtoValidator>();
        services.AddScoped<IValidator<LongTermGoalUpdateDto>, LongTermGoalUpdateDtoValidator>();
        services.AddScoped<IValidator<CreditCardCreateDto>, CreditCardCreateDtoValidator>();
        services.AddScoped<IValidator<CreditCardUpdateDto>, CreditCardUpdateDtoValidator>();

        return services;
    }
}
