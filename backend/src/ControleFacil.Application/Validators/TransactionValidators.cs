using ControleFacil.Application.Dtos;
using FluentValidation;

namespace ControleFacil.Application.Validators;

public class TransactionCreateDtoValidator : AbstractValidator<TransactionCreateDto>
{
    public TransactionCreateDtoValidator()
    {
        RuleFor(x => x.EntryDate).NotEqual(default(DateOnly)).WithMessage("Data do lançamento é obrigatória.");
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.BankAccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.TotalInstallments).InclusiveBetween(1, 60).When(x => x.TotalInstallments.HasValue);
    }
}

public class TransactionUpdateDtoValidator : AbstractValidator<TransactionUpdateDto>
{
    public TransactionUpdateDtoValidator()
    {
        RuleFor(x => x.EntryDate).NotEqual(default(DateOnly)).WithMessage("Data do lançamento é obrigatória.");
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.BankAccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Status).IsInEnum();
    }
}
