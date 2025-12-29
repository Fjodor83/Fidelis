using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Coupons.Queries.GetCouponById;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, Result<CouponDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCouponByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CouponDto>> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (coupon == null)
            return Result<CouponDto>.Failure("Coupon non trovato.");

        return Result<CouponDto>.Success(new CouponDto
        {
            Id = coupon.Id,
            Codice = coupon.Codice,
            Titolo = coupon.Titolo,
            Descrizione = coupon.Descrizione,
            ValoreSconto = coupon.ValoreSconto,
            TipoSconto = coupon.TipoSconto.ToString(),
            DataInizio = coupon.DataInizio,
            DataScadenza = coupon.DataScadenza,
            Attivo = coupon.Attivo,
            ImportoMinimoOrdine = coupon.ImportoMinimoOrdine,
            LimiteUtilizzoGlobale = coupon.LimiteUtilizzoGlobale,
            UtilizziTotali = coupon.UtilizziTotali
        });
    }
}
