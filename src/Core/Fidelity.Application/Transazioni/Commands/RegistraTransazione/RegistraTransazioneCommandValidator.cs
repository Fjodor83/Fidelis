using FluentValidation;

namespace Fidelity.Application.Transazioni.Commands.RegistraTransazione;

public class RegistraTransazioneCommandValidator : AbstractValidator<RegistraTransazioneCommand>
{
    public RegistraTransazioneCommandValidator()
    {
        RuleFor(v => v.ClienteId)
            .GreaterThan(0).WithMessage("ClienteId è obbligatorio");

        RuleFor(v => v.PuntoVenditaId)
            .GreaterThan(0).WithMessage("PuntoVenditaId è obbligatorio");

        RuleFor(v => v.Importo)
            .GreaterThan(0).WithMessage("Importo deve essere maggiore di zero")
            .LessThan(100000).WithMessage("Importo troppo elevato");

        RuleFor(v => v.Note)
            .MaximumLength(500).WithMessage("Note non possono superare 500 caratteri")
            .When(v => !string.IsNullOrEmpty(v.Note));
    }
}
