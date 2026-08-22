using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class LongTermGoalCreateDtoValidator : AbstractValidator<LongTermGoalCreateDto>
{
    public LongTermGoalCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.TargetYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.TargetMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.InvestmentCategoryId).GreaterThan(0).When(x => x.InvestmentCategoryId.HasValue);
        RuleFor(x => x.ManualCurrentAmount).GreaterThanOrEqualTo(0);
    }
}

public class LongTermGoalUpdateDtoValidator : AbstractValidator<LongTermGoalUpdateDto>
{
    public LongTermGoalUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.TargetYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.TargetMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.InvestmentCategoryId).GreaterThan(0).When(x => x.InvestmentCategoryId.HasValue);
        RuleFor(x => x.ManualCurrentAmount).GreaterThanOrEqualTo(0);
    }
}
