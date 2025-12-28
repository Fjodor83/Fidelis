using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fidelity.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clienti => Set<Cliente>();
    public DbSet<Transazione> Transazioni => Set<Transazione>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponAssegnato> CouponsAssegnati => Set<CouponAssegnato>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}
