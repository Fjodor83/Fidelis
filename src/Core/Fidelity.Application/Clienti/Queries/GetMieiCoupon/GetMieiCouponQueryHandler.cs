using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Coupons.Queries.GetMieiCoupon;

public class GetMieiCouponQueryHandler : IRequestHandler<GetMieiCouponQuery, Result<List<CouponAssegnatoDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMieiCouponQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CouponAssegnatoDto>>> Handle(
        GetMieiCouponQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.CouponAssegnati
            .Include(ca => ca.Coupon)
            .Where(ca => ca.ClienteId == request.ClienteId)
            .AsNoTracking();

        if (request.SoloAttivi)
        {
            var now = DateTime.UtcNow;
            query = query.Where(ca => !ca.Utilizzato && ca.Coupon.DataScadenza > now && ca.Coupon.Attivo);
        }

        var coupons = await query
            .OrderByDescending(ca => ca.DataAssegnazione)
            .Select(ca => new CouponAssegnatoDto
            {
                Id = ca.Id,
                CouponId = ca.CouponId,
                Codice = ca.Coupon.Codice,
                Titolo = ca.Coupon.Titolo,
                Descrizione = ca.Coupon.Descrizione,
                ValoreSconto = ca.Coupon.ValoreSconto,
                TipoSconto = ca.Coupon.TipoSconto.ToString(),
                DataInizio = ca.Coupon.DataInizio,
                DataScadenza = ca.Coupon.DataScadenza,
                DataAssegnazione = ca.DataAssegnazione,
                Utilizzato = ca.Utilizzato,
                DataUtilizzo = ca.DataUtilizzo,
                ImportoMinimoOrdine = ca.Coupon.ImportoMinimoOrdine
            })
            .ToListAsync(cancellationToken);

        return Result<List<CouponAssegnatoDto>>.Success(coupons);
    }
}
