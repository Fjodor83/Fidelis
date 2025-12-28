using FluentValidation;

namespace Fidelity.Application.Clienti.Commands.RegistraCliente;

public class RegistraClienteCommandValidator : AbstractValidator<RegistraClienteCommand>
{
    public RegistraClienteCommandValidator()
    {
        RuleFor(v => v.Nome)
            .NotEmpty().WithMessage("Nome è obbligatorio")
            .MaximumLength(100).WithMessage("Nome non può superare 100 caratteri");

        RuleFor(v => v.Cognome)
            .NotEmpty().WithMessage("Cognome è obbligatorio")
            .MaximumLength(100).WithMessage("Cognome non può superare 100 caratteri");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email è obbligatoria")
            .EmailAddress().WithMessage("Email non valida")
            .MaximumLength(255).WithMessage("Email non può superare 255 caratteri");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password è obbligatoria")
            .MinimumLength(6).WithMessage("Password deve essere almeno 6 caratteri");

        RuleFor(v => v.PrivacyAccepted)
            .Equal(true).WithMessage("Devi accettare la privacy policy");

        RuleFor(v => v.Telefono)
            .MaximumLength(20).WithMessage("Telefono non può superare 20 caratteri")
            .When(v => !string.IsNullOrEmpty(v.Telefono));
    }
}
