using Fidelity.Application.Common.Behaviors;
using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Common;
using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fidelity.Infrastructure.Persistence;

/// <summary>
/// Clean Architecture Application DbContext - ISO 25000: Maintainability, Reliability
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly DomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        DomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _currentUserService = currentUserService;
        _domainEventDispatcher = domainEventDispatcher;
    }

    // Core entities
    public DbSet<Cliente> Clienti => Set<Cliente>();
    public DbSet<PuntoVendita> PuntiVendita => Set<PuntoVendita>();
    public DbSet<Responsabile> Responsabili => Set<Responsabile>();
    public DbSet<ResponsabilePuntoVendita> ResponsabilePuntiVendita => Set<ResponsabilePuntoVendita>();

    // Fidelity program
    public DbSet<Transazione> Transazioni => Set<Transazione>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponAssegnato> CouponAssegnati => Set<CouponAssegnato>();

    // Registration
    public DbSet<TokenRegistrazione> TokenRegistrazione => Set<TokenRegistrazione>();

    // Authentication
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    // System
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        modelBuilder.Entity<Cliente>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PuntoVendita>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Responsabile>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Coupon>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields
        UpdateAuditFields();

        // Dispatch domain events before saving
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        if (_domainEventDispatcher != null && entities.Any())
        {
            await _domainEventDispatcher.DispatchEventsAsync(entities, cancellationToken);
        }

        return result;
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentUser = _currentUserService?.Username ?? "System";
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser;
                    break;
            }
        }

        // Handle soft delete entities
        var softDeleteEntries = ChangeTracker.Entries<SoftDeleteEntity>();
        foreach (var entry in softDeleteEntries)
        {
            if (entry.State == EntityState.Deleted)
            {
                // Convert to soft delete
                entry.State = EntityState.Modified;
                entry.Entity.SoftDelete(currentUser);
            }
        }
    }
}
