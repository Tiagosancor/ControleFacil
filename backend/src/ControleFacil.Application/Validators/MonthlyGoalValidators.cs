using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class MonthlyGoalCreateDtoValidator : AbstractValidator<MonthlyGoalCreateDto>
{
    public MonthlyGoalCreateDtoValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.IncomeGoal).GreaterThan(0);
        RuleFor(x => x.ExpenseGoal).GreaterThan(0);
    }
}

public class MonthlyGoalUpdateDtoValidator : AbstractValidator<MonthlyGoalUpdateDto>
{
    public MonthlyGoalUpdateDtoValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.IncomeGoal).GreaterThan(0);
        RuleFor(x => x.ExpenseGoal).GreaterThan(0);
    }
}
