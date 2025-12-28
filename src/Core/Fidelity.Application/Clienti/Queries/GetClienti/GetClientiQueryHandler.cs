using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Clienti.Queries.GetClienti;

public class GetClientiQueryHandler : IRequestHandler<GetClientiQuery, List<ClienteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientiQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClienteDto>> Handle(GetClientiQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Clienti.AsQueryable();

        // Filter by active status
        if (request.SoloAttivi.HasValue && request.SoloAttivi.Value)
        {
            query = query.Where(c => c.Attivo);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Nome.ToLower().Contains(searchTerm) ||
                c.Cognome.ToLower().Contains(searchTerm) ||
                c.Email.ToLower().Contains(searchTerm) ||
                c.CodiceFidelity.ToLower().Contains(searchTerm)
            );
        }

        var clienti = await query
            .OrderByDescending(c => c.DataRegistrazione)
            .Select(c => new ClienteDto
            {
                Id = c.Id,
                CodiceFidelity = c.CodiceFidelity,
                Nome = c.Nome,
                Cognome = c.Cognome,
                Email = c.Email,
                Telefono = c.Telefono,
                DataRegistrazione = c.DataRegistrazione,
                PuntiTotali = c.PuntiTotali,
                Attivo = c.Attivo
            })
            .ToListAsync(cancellationToken);

        return clienti;
    }
}
