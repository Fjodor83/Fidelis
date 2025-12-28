using FluentValidation;

namespace Fidelity.Application.Coupons.Commands.CreaCoupon;

public class CreaCouponCommandValidator : AbstractValidator<CreaCouponCommand>
{
    public CreaCouponCommandValidator()
    {
        RuleFor(v => v.Codice)
            .NotEmpty().WithMessage("Codice è obbligatorio")
            .MaximumLength(50);

        RuleFor(v => v.Titolo)
            .NotEmpty().WithMessage("Titolo è obbligatorio")
            .MaximumLength(200);

        RuleFor(v => v.ValoreSconto)
            .GreaterThan(0).WithMessage("Valore sconto deve essere maggiore di zero");

        RuleFor(v => v.TipoSconto)
            .Must(t => t == "Percentuale" || t == "Fisso")
            .WithMessage("Tipo sconto deve essere 'Percentuale' o 'Fisso'");

        RuleFor(v => v.DataScadenza)
            .GreaterThan(v => v.DataInizio)
            .WithMessage("Data scadenza deve essere successiva a data inizio");
    }
}
