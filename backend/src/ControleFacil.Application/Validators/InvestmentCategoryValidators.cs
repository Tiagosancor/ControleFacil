using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class InvestmentCategoryCreateDtoValidator : AbstractValidator<InvestmentCategoryCreateDto>
{
    public InvestmentCategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class InvestmentCategoryUpdateDtoValidator : AbstractValidator<InvestmentCategoryUpdateDto>
{
    public InvestmentCategoryUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
