using FluentValidation;

namespace Fidelity.Application.Transazioni.Commands.AssegnaPunti;

public class AssegnaPuntiCommandValidator : AbstractValidator<AssegnaPuntiCommand>
{
    public AssegnaPuntiCommandValidator()
    {
        RuleFor(x => x.ClienteId)
            .GreaterThan(0).WithMessage("Cliente non valido");

        RuleFor(x => x.ImportoSpesa)
            .GreaterThan(0).WithMessage("L'importo spesa deve essere maggiore di zero");

        RuleFor(x => x.PuntoVenditaId)
            .GreaterThan(0).WithMessage("Punto vendita non valido");

        RuleFor(x => x.ResponsabileId)
            .GreaterThan(0).WithMessage("Responsabile non valido");
    }
}
