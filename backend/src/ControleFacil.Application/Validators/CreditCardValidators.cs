using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class CreditCardCreateDtoValidator : AbstractValidator<CreditCardCreateDto>
{
    public CreditCardCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClosingDay).InclusiveBetween(1, 31);
        RuleFor(x => x.DueDay).InclusiveBetween(1, 31);
    }
}

public class CreditCardUpdateDtoValidator : AbstractValidator<CreditCardUpdateDto>
{
    public CreditCardUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ClosingDay).InclusiveBetween(1, 31);
        RuleFor(x => x.DueDay).InclusiveBetween(1, 31);
    }
}
