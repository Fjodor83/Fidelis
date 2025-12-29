using FluentValidation;

namespace Fidelity.Application.Clienti.Commands.RegistraCliente;

public class RegistraClienteCommandValidator : AbstractValidator<RegistraClienteCommand>
{
    public RegistraClienteCommandValidator()
    {
        When(x => !x.HasExistingCard, () =>
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Il nome è obbligatorio")
                .MaximumLength(100).WithMessage("Il nome non può superare i 100 caratteri");

            RuleFor(x => x.Cognome)
                .NotEmpty().WithMessage("Il cognome è obbligatorio")
                .MaximumLength(100).WithMessage("Il cognome non può superare i 100 caratteri");
        });

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email è obbligatoria")
            .EmailAddress().WithMessage("Formato email non valido")
            .MaximumLength(255).WithMessage("L'email non può superare i 255 caratteri");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La password è obbligatoria")
            .MinimumLength(6).WithMessage("La password deve avere almeno 6 caratteri")
            .MaximumLength(100).WithMessage("La password non può superare i 100 caratteri");

        RuleFor(x => x.Telefono)
            .MaximumLength(20).WithMessage("Il telefono non può superare i 20 caratteri")
            .When(x => !string.IsNullOrEmpty(x.Telefono));

        RuleFor(x => x.PrivacyAccepted)
            .Equal(true).WithMessage("Devi accettare la privacy policy per procedere");

        When(x => x.HasExistingCard, () =>
        {
            RuleFor(x => x.ExistingFidelityCode)
                .NotEmpty().WithMessage("Il codice fidelity è obbligatorio per attivare una card esistente")
                .Length(12).WithMessage("Il codice fidelity deve essere di 12 caratteri")
                .Matches("^SUN[0-9]{9}$").WithMessage("Formato codice fidelity non valido");
        });
    }
}
