using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class InvestmentEntryCreateDtoValidator : AbstractValidator<InvestmentEntryCreateDto>
{
    public InvestmentEntryCreateDtoValidator()
    {
        RuleFor(x => x.InvestmentCategoryId).GreaterThan(0);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
    }
}

public class InvestmentEntryUpdateDtoValidator : AbstractValidator<InvestmentEntryUpdateDto>
{
    public InvestmentEntryUpdateDtoValidator()
    {
        RuleFor(x => x.InvestmentCategoryId).GreaterThan(0);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
    }
}
