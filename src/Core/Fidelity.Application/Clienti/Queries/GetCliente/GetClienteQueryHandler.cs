using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Clienti.Queries.GetCliente;

public class GetClienteQueryHandler :
    IRequestHandler<GetClienteQuery, Result<ClienteDetailDto>>,
    IRequestHandler<GetClienteByCodiceFidelityQuery, Result<ClienteDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClienteQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClienteDetailDto>> Handle(
        GetClienteQuery request,
        CancellationToken cancellationToken)
    {
        var cliente = await _context.Clienti
            .Include(c => c.PuntoVenditaRegistrazione)
            .Include(c => c.Transazioni.OrderByDescending(t => t.DataTransazione).Take(10))
            .Include(c => c.CouponAssegnati.Where(ca => !ca.Utilizzato))
                .ThenInclude(ca => ca.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ClienteId && !c.IsDeleted, cancellationToken);

        if (cliente == null)
            return Result<ClienteDetailDto>.Failure("Cliente non trovato.");

        return Result<ClienteDetailDto>.Success(MapToDto(cliente));
    }

    public async Task<Result<ClienteDetailDto>> Handle(
        GetClienteByCodiceFidelityQuery request,
        CancellationToken cancellationToken)
    {
        var cliente = await _context.Clienti
            .Include(c => c.PuntoVenditaRegistrazione)
            .Include(c => c.Transazioni.OrderByDescending(t => t.DataTransazione).Take(10))
            .Include(c => c.CouponAssegnati.Where(ca => !ca.Utilizzato))
                .ThenInclude(ca => ca.Coupon)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CodiceFidelity == request.CodiceFidelity.ToUpper() && !c.IsDeleted, cancellationToken);

        if (cliente == null)
            return Result<ClienteDetailDto>.Failure("Cliente non trovato.");

        return Result<ClienteDetailDto>.Success(MapToDto(cliente));
    }

    private static ClienteDetailDto MapToDto(Domain.Entities.Cliente cliente)
    {
        return new ClienteDetailDto
        {
            Id = cliente.Id,
            CodiceFidelity = cliente.CodiceFidelity,
            Nome = cliente.Nome,
            Cognome = cliente.Cognome,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            DataRegistrazione = cliente.DataRegistrazione,
            PuntiTotali = cliente.PuntiTotali,
            PuntiDisponibili = cliente.PuntiDisponibili,
            Livello = cliente.Livello.ToString(),
            Attivo = cliente.Attivo,
            PuntoVenditaRegistrazione = cliente.PuntoVenditaRegistrazione?.Nome,
            UltimeTransazioni = cliente.Transazioni.Select(t => new TransazioneDto
            {
                Id = t.Id,
                ClienteId = t.ClienteId,
                DataTransazione = t.DataTransazione,
                Importo = t.ImportoSpesa,
                PuntiGuadagnati = t.PuntiAssegnati,
                Tipo = "Accumulo"
            }).ToList(),
            CouponAttivi = cliente.CouponAssegnati
                .Where(ca => !ca.Utilizzato && ca.Coupon.IsValido())
                .Select(ca => new CouponAssegnatoDto
                {
                    Id = ca.Id,
                    Codice = ca.Coupon.Codice,
                    Titolo = ca.Coupon.Titolo,
                    ValoreSconto = ca.Coupon.ValoreSconto,
                    TipoSconto = ca.Coupon.TipoSconto.ToString(),
                    DataScadenza = ca.Coupon.DataScadenza,
                    DataAssegnazione = ca.DataAssegnazione
                }).ToList()
        };
    }
}
