using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Cliente> Clienti { get; }
    DbSet<Transazione> Transazioni { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponAssegnato> CouponsAssegnati { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
