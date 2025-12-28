using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Transazioni.Queries.GetTransazioniCliente;

public class GetTransazioniClienteQueryHandler : IRequestHandler<GetTransazioniClienteQuery, List<TransazioneDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransazioniClienteQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransazioneDto>> Handle(GetTransazioniClienteQuery request, CancellationToken cancellationToken)
    {
        var transazioni = await _context.Transazioni
            .Where(t => t.ClienteId == request.ClienteId)
            .OrderByDescending(t => t.DataTransazione)
            .Select(t => new TransazioneDto
            {
                Id = t.Id,
                ClienteId = t.ClienteId,
                ClienteNome = "", // Will be populated by join if needed
                PuntoVenditaId = t.PuntoVenditaId,
                PuntoVenditaNome = "", // Will be populated by join if needed
                ResponsabileId = t.ResponsabileId,
               ResponsabileNome = null,
                DataTransazione = t.DataTransazione,
                Importo = t.Importo,
                PuntiGuadagnati = t.PuntiGuadagnati,
                Note = t.Note
            })
            .ToListAsync(cancellationToken);

        return transazioni;
    }
}
