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
        var mesePrecedente = now.AddMonths(-1);

        var stats = new DashboardStatsDto
        {
            // Clienti stats
            TotaleClienti = await _context.Clienti.CountAsync(cancellationToken),
            ClientiAttivi = await _context.Clienti.CountAsync(c => c.Attivo, cancellationToken),
            TotalePuntiDistribuiti = await _context.Clienti.SumAsync(c => c.PuntiTotali, cancellationToken),
            
            // Transazioni del mese
            TotaleTransazioniMese = await _context.Transazioni
                .Where(t => t.DataTransazione >= mesePrecedente)
                .SumAsync(t => t.Importo, cancellationToken),
            
            NumeroTransazioniMese = await _context.Transazioni
                .CountAsync(t => t.DataTransazione >= mesePrecedente, cancellationToken),
            
            // Coupons attivi
            CouponsAttivi = await _context.Coupons
                .CountAsync(c => c.Attivo && c.DataScadenza > now, cancellationToken)
        };

        return stats;
    }
}
