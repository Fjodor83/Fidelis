using FluentValidation;

namespace Fidelity.Application.Coupons.Commands.CreaCoupon;

public class CreaCouponCommandValidator : AbstractValidator<CreaCouponCommand>
{
    public CreaCouponCommandValidator()
    {
        RuleFor(x => x.Codice)
            .NotEmpty().WithMessage("Il codice è obbligatorio")
            .MaximumLength(20).WithMessage("Il codice non può superare i 20 caratteri")
            .Matches("^[A-Z0-9]+$").WithMessage("Il codice può contenere solo lettere maiuscole e numeri");

        RuleFor(x => x.Titolo)
            .NotEmpty().WithMessage("Il titolo è obbligatorio")
            .MaximumLength(200).WithMessage("Il titolo non può superare i 200 caratteri");

        RuleFor(x => x.Descrizione)
            .MaximumLength(500).WithMessage("La descrizione non può superare i 500 caratteri");

        RuleFor(x => x.ValoreSconto)
            .GreaterThan(0).WithMessage("Il valore sconto deve essere maggiore di zero");

        RuleFor(x => x.ValoreSconto)
            .LessThanOrEqualTo(100)
            .When(x => x.TipoSconto == "Percentuale")
            .WithMessage("Lo sconto percentuale non può superare il 100%");

        RuleFor(x => x.TipoSconto)
            .Must(t => t == "Percentuale" || t == "Fisso")
            .WithMessage("Il tipo sconto deve essere 'Percentuale' o 'Fisso'");

        RuleFor(x => x.DataInizio)
            .NotEmpty().WithMessage("La data inizio è obbligatoria");

        RuleFor(x => x.DataScadenza)
            .NotEmpty().WithMessage("La data scadenza è obbligatoria")
            .GreaterThan(x => x.DataInizio).WithMessage("La data scadenza deve essere successiva alla data inizio");

        RuleFor(x => x.LimiteUtilizzoGlobale)
            .GreaterThan(0).When(x => x.LimiteUtilizzoGlobale.HasValue)
            .WithMessage("Il limite utilizzo globale deve essere maggiore di zero");

        RuleFor(x => x.ImportoMinimoOrdine)
            .GreaterThan(0).When(x => x.ImportoMinimoOrdine.HasValue)
            .WithMessage("L'importo minimo ordine deve essere maggiore di zero");
    }
}
