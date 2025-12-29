using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codice)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(c => c.Codice)
            .IsUnique();

        builder.Property(c => c.Titolo)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Descrizione)
            .HasMaxLength(500);

        builder.Property(c => c.ValoreSconto)
            .HasPrecision(5, 2);

        builder.Property(c => c.TipoSconto)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TipoSconto.Percentuale);

        builder.Property(c => c.ImportoMinimoOrdine)
            .HasPrecision(10, 2);

        builder.Property(c => c.LivelloMinimoRichiesto)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(c => c.DataScadenza);
        builder.HasIndex(c => new { c.Attivo, c.IsDeleted });
    }
}
