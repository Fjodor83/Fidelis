using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Transazioni.Commands.RegistraTransazione;

public class RegistraTransazioneCommandHandler : IRequestHandler<RegistraTransazioneCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public RegistraTransazioneCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(RegistraTransazioneCommand request, CancellationToken cancellationToken)
    {
        // Verify cliente exists
        var cliente = await _context.Clienti
            .FindAsync(new object[] { request.ClienteId }, cancellationToken);
        
        if (cliente == null)
            return Result<int>.Failure($"Cliente con ID {request.ClienteId} non trovato");
        
        if (!cliente.Attivo)
            return Result<int>.Failure("Il cliente non è attivo");
        
        // Calculate points
        var puntiGuadagnati = Transazione.CalcolaPunti(request.Importo);
        
        // Create transaction
        var transazione = new Transazione
        {
            ClienteId = request.ClienteId,
            PuntoVenditaId = request.PuntoVenditaId,
            ResponsabileId = request.ResponsabileId,
            DataTransazione = DateTime.UtcNow,
            Importo = request.Importo,
            PuntiGuadagnati = puntiGuadagnati,
            Note = request.Note
        };
        
        // Validate business rules
        try
        {
            transazione.Validate();
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
        
        // Update cliente points using domain method
        cliente.AggiungiPunti(puntiGuadagnati);
        
        _context.Transazioni.Add(transazione);
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result<int>.Success(transazione.Id);
    }
}
