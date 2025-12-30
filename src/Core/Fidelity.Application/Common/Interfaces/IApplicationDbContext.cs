using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Application.Common.Interfaces;

/// <summary>
/// Application Database Context interface - ISO 25000: Maintainability
/// </summary>
public interface IApplicationDbContext
{
    // Core entities
    DbSet<Cliente> Clienti { get; }
    DbSet<PuntoVendita> PuntiVendita { get; }
    DbSet<Responsabile> Responsabili { get; }
    DbSet<ResponsabilePuntoVendita> ResponsabilePuntiVendita { get; }

    // Fidelity program
    DbSet<Transazione> Transazioni { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<CouponAssegnato> CouponAssegnati { get; }

    // Registration
    DbSet<TokenRegistrazione> TokenRegistrazione { get; }

    // Authentication
    DbSet<RefreshToken> RefreshTokens { get; }
    
    // System
    DbSet<IdempotencyRecord> IdempotencyRecords { get; }

    // Operations
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
