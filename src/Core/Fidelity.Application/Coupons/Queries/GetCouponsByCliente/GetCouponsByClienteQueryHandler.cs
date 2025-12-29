using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Coupons.Queries.GetCouponsByCliente;

public class GetCouponsByClienteQueryHandler : IRequestHandler<GetCouponsByClienteQuery, List<CouponAssegnatoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCouponsByClienteQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CouponAssegnatoDto>> Handle(GetCouponsByClienteQuery request, CancellationToken cancellationToken)
    {
        return await _context.CouponAssegnati
            .Include(ca => ca.Coupon)
            .AsNoTracking()
            .Where(ca => ca.ClienteId == request.ClienteId && !ca.Coupon.IsDeleted) // and maybe only valid? No, all assigned.
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
    }
}
