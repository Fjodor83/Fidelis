using AutoMapper;
using Fidelity.Server.Data;
using Fidelity.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Fidelity.Server.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public AnalyticsService(ApplicationDbContext context, IMapper mapper, IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<DashboardStatsDTO> GetStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var cacheKey = "dashboard_stats";

            if (!_cache.TryGetValue(cacheKey, out DashboardStatsDTO? stats))
            {
                stats = new DashboardStatsDTO
                {
                    TotaleClienti = await _context.Clienti.CountAsync(),
                    ClientiRegistratiOggi = await _context.Clienti.CountAsync(c => c.DataRegistrazione >= today),
                    PuntiTotaliEmessi = await _context.Clienti.SumAsync(c => c.PuntiTotali),
                    CouponAttivi = await _context.Coupons.CountAsync(c => c.Attivo),
                    CouponRiscattati = await _context.CouponAssegnati.CountAsync(c => c.Utilizzato),
                    TransazioniOggi = await _context.Transazioni.CountAsync(m => m.DataTransazione >= today)
                };

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, stats, cacheEntryOptions);
            }

            return stats!;
        }

        public async Task<List<RegistrationStatsDTO>> GetRegistrationHistoryAsync()
        {
             var last30Days = DateTime.UtcNow.AddDays(-30);
            var cacheKey = "registration_history";

            if (!_cache.TryGetValue(cacheKey, out List<RegistrationStatsDTO>? result))
            {
                var history = await _context.Clienti
                    .Where(c => c.DataRegistrazione >= last30Days)
                    .GroupBy(c => c.DataRegistrazione.Date)
                    .Select(g => new RegistrationStatsDTO
                    {
                        Data = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Data)
                    .ToListAsync();

                result = new List<RegistrationStatsDTO>();
                for (var i = 0; i < 30; i++)
                {
                    var date = last30Days.AddDays(i).Date;
                    var existing = history.FirstOrDefault(h => h.Data.Date == date);
                    result.Add(existing ?? new RegistrationStatsDTO { Data = date, Count = 0 });
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(cacheKey, result, cacheEntryOptions);
            }

            return result!;
        }

        public async Task<List<RecentActivityDTO>> GetRecentActivityAsync()
        {
            var ultimiPunti = await _context.Transazioni
                .Include(m => m.Cliente)
                .Include(m => m.PuntoVendita)
                .OrderByDescending(m => m.DataTransazione)
                .Take(10)
                .ToListAsync();

            var ultimiPuntiDTO = _mapper.Map<List<RecentActivityDTO>>(ultimiPunti);

            var ultimiCoupon = await _context.CouponAssegnati
                .Include(ca => ca.Cliente)
                .Include(ca => ca.Coupon)
                .Where(ca => ca.Utilizzato && ca.DataUtilizzo.HasValue)
                .OrderByDescending(ca => ca.DataUtilizzo)
                .Take(10)
                .ToListAsync();

            var ultimiCouponDTO = _mapper.Map<List<RecentActivityDTO>>(ultimiCoupon);

            var activity = ultimiPuntiDTO.Concat(ultimiCouponDTO)
                .OrderByDescending(x => x.Data)
                .Take(10)
                .ToList();

            return activity;
        }
    }
}
