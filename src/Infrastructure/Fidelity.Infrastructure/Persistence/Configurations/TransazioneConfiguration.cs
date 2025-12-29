using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class TransazioneConfiguration : IEntityTypeConfiguration<Transazione>
{
    public void Configure(EntityTypeBuilder<Transazione> builder)
    {
        builder.ToTable("Transazioni");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ImportoSpesa)
            .HasPrecision(18, 2);

        builder.Property(t => t.Note)
            .HasMaxLength(500);

        builder.Property(t => t.Tipo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TipoTransazione.Accumulo);

        // Relationships
        builder.HasOne(t => t.Cliente)
            .WithMany(c => c.Transazioni)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.PuntoVendita)
            .WithMany(p => p.Transazioni)
            .HasForeignKey(t => t.PuntoVenditaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Responsabile)
            .WithMany(r => r.Transazioni)
            .HasForeignKey(t => t.ResponsabileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CouponAssegnato)
            .WithOne(ca => ca.TransazioneUtilizzo)
            .HasForeignKey<Transazione>(t => t.CouponAssegnatoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.DataTransazione);
        builder.HasIndex(t => new { t.ClienteId, t.DataTransazione });
    }
}
