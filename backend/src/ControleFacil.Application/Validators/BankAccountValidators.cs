using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class BankAccountCreateDtoValidator : AbstractValidator<BankAccountCreateDto>
{
    public BankAccountCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}

public class BankAccountUpdateDtoValidator : AbstractValidator<BankAccountUpdateDto>
{
    public BankAccountUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InitialBalance).GreaterThanOrEqualTo(0);
    }
}
