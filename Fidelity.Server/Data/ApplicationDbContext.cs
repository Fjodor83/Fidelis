// Fidelity.Server/Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using Fidelity.Shared.Models;

namespace Fidelity.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clienti { get; set; }
        public DbSet<PuntoVendita> PuntiVendita { get; set; }
        public DbSet<Responsabile> Responsabili { get; set; }
        public DbSet<ResponsabilePuntoVendita> ResponsabilePuntiVendita { get; set; }
        public DbSet<TokenRegistrazione> TokenRegistrazione { get; set; }
        public DbSet<Transazione> Transazioni { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponAssegnato> CouponAssegnati { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Indici unici
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CodiceFidelity)
                .IsUnique();

            modelBuilder.Entity<PuntoVendita>()
                .HasIndex(p => p.Codice)
                .IsUnique();

            modelBuilder.Entity<Responsabile>()
                .HasIndex(r => r.Username)
                .IsUnique();

            modelBuilder.Entity<TokenRegistrazione>()
                .HasIndex(t => t.Token)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Codice)
                .IsUnique();

            // Relazioni
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.PuntoVenditaRegistrazione)
                .WithMany(p => p.ClientiRegistrati)
                .HasForeignKey(c => c.PuntoVenditaRegistrazioneId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.ResponsabileRegistrazione)
                .WithMany()
                .HasForeignKey(c => c.ResponsabileRegistrazioneId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResponsabilePuntoVendita>()
                .HasKey(rp => new { rp.ResponsabileId, rp.PuntoVenditaId });

            modelBuilder.Entity<ResponsabilePuntoVendita>()
                .HasOne(rp => rp.Responsabile)
                .WithMany(r => r.ResponsabilePuntiVendita)
                .HasForeignKey(rp => rp.ResponsabileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ResponsabilePuntoVendita>()
                .HasOne(rp => rp.PuntoVendita)
                .WithMany(pv => pv.ResponsabilePuntiVendita)
                .HasForeignKey(rp => rp.PuntoVenditaId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
            
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Cliente)
                .WithMany()
                .HasForeignKey(rt => rt.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Responsabile)
                .WithMany()
                .HasForeignKey(rt => rt.ResponsabileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}