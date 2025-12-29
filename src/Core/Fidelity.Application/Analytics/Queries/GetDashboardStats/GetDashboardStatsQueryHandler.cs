using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Analytics.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var oggi = DateTime.Today;

        var stats = new DashboardStatsDto
        {
            TotaleClienti = await _context.Clienti.CountAsync(cancellationToken),
            ClientiRegistratiOggi = await _context.Clienti
                .CountAsync(c => c.DataRegistrazione >= oggi, cancellationToken),
            
            PuntiTotaliEmessi = await _context.Transazioni
                .SumAsync(t => (int?)t.PuntiAssegnati ?? 0, cancellationToken),
            
            CouponAttivi = await _context.Coupons
                .CountAsync(c => c.Attivo && c.DataScadenza > now, cancellationToken),
            
            CouponRiscattati = await _context.CouponAssegnati
                .CountAsync(c => c.Utilizzato, cancellationToken),
            
            TransazioniOggi = await _context.Transazioni
                .CountAsync(t => t.DataTransazione >= oggi, cancellationToken)
        };

        return stats;
    }
}
