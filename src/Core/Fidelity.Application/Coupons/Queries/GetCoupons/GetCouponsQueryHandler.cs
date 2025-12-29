using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Coupons.Queries.GetCoupons;

public class GetCouponsQueryHandler : 
    IRequestHandler<GetCouponsQuery, List<CouponDto>>,
    IRequestHandler<GetCouponsDisponibiliQuery, List<CouponDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCouponsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CouponDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.DataScadenza)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CouponDto>> Handle(GetCouponsDisponibiliQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await _context.Coupons
            .AsNoTracking()
            .Where(c => c.Attivo && c.DataInizio <= now && c.DataScadenza >= now)
            .OrderByDescending(c => c.DataScadenza)
            .Select(c => MapToDto(c))
            .ToListAsync(cancellationToken);
    }

    private static CouponDto MapToDto(Domain.Entities.Coupon c)
    {
        return new CouponDto
        {
            Id = c.Id,
            Codice = c.Codice,
            Titolo = c.Titolo,
            Descrizione = c.Descrizione,
            ValoreSconto = c.ValoreSconto,
            TipoSconto = c.TipoSconto.ToString(),
            DataInizio = c.DataInizio,
            DataScadenza = c.DataScadenza,
            Attivo = c.Attivo,
            ImportoMinimoOrdine = c.ImportoMinimoOrdine,
            LimiteUtilizzoGlobale = c.LimiteUtilizzoGlobale,
            UtilizziTotali = c.UtilizziTotali
        };
    }
}
