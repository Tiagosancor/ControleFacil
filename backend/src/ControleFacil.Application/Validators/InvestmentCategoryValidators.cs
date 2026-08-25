using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class InvestmentCategoryCreateDtoValidator : AbstractValidator<InvestmentCategoryCreateDto>
{
    public InvestmentCategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.AppliedAmount).GreaterThan(0);
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0).When(x => x.InterestRate.HasValue);
        RuleFor(x => x.MonthlyContribution).GreaterThanOrEqualTo(0).When(x => x.MonthlyContribution.HasValue);
    }
}

public class InvestmentCategoryUpdateDtoValidator : AbstractValidator<InvestmentCategoryUpdateDto>
{
    public InvestmentCategoryUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.AppliedAmount).GreaterThan(0);
        RuleFor(x => x.InterestRate).GreaterThanOrEqualTo(0).When(x => x.InterestRate.HasValue);
        RuleFor(x => x.MonthlyContribution).GreaterThanOrEqualTo(0).When(x => x.MonthlyContribution.HasValue);
    }
}
