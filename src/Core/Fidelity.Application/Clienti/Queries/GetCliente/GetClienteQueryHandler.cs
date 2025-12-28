using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Clienti.Queries.GetCliente;

public class GetClienteQueryHandler : IRequestHandler<GetClienteQuery, ClienteDto?>
{
    private readonly IApplicationDbContext _context;

    public GetClienteQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClienteDto?> Handle(GetClienteQuery request, CancellationToken cancellationToken)
    {
        var cliente = await _context.Clienti
            .Where(c => c.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return cliente;
    }
}
