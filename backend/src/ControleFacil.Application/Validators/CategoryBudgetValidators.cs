using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class CategoryBudgetCreateDtoValidator : AbstractValidator<CategoryBudgetCreateDto>
{
    public CategoryBudgetCreateDtoValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}

public class CategoryBudgetUpdateDtoValidator : AbstractValidator<CategoryBudgetUpdateDto>
{
    public CategoryBudgetUpdateDtoValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.LimitAmount).GreaterThan(0);
    }
}
