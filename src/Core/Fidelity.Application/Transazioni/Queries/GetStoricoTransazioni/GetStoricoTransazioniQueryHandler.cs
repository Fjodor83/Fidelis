using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Transazioni.Queries.GetStoricoTransazioni;

public class GetStoricoTransazioniQueryHandler : IRequestHandler<GetStoricoTransazioniQuery, List<TransazioneDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStoricoTransazioniQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransazioneDto>> Handle(GetStoricoTransazioniQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Transazioni
            .Include(t => t.Cliente)
            .Include(t => t.PuntoVendita)
            .Include(t => t.Responsabile)
            .AsNoTracking();

        if (request.PuntoVenditaId.HasValue && request.PuntoVenditaId.Value > 0)
        {
            query = query.Where(t => t.PuntoVenditaId == request.PuntoVenditaId.Value);
        }

        if (request.ClienteId.HasValue)
        {
            query = query.Where(t => t.ClienteId == request.ClienteId.Value);
        }

        if (request.DataInizio.HasValue)
        {
            query = query.Where(t => t.DataTransazione >= request.DataInizio.Value);
        }

        if (request.DataFine.HasValue)
        {
            query = query.Where(t => t.DataTransazione <= request.DataFine.Value);
        }

        var transazioni = await query
            .OrderByDescending(t => t.DataTransazione)
            .Take(request.Limit)
            .Select(t => new TransazioneDto
            {
                Id = t.Id,
                ClienteId = t.ClienteId,
                ClienteNome = $"{t.Cliente.Nome} {t.Cliente.Cognome}",
                CodiceFidelity = t.Cliente.CodiceFidelity,
                PuntoVenditaNome = t.PuntoVendita.Nome,
                ResponsabileNome = t.Responsabile != null ? (t.Responsabile.NomeCompleto ?? t.Responsabile.Username) : "Sistema",
                DataTransazione = t.DataTransazione,
                Importo = t.ImportoSpesa,
                PuntiGuadagnati = t.PuntiAssegnati,
                Tipo = t.Tipo.ToString(),
                Note = t.Note
            })
            .ToListAsync(cancellationToken);

        return transazioni;
    }
}
