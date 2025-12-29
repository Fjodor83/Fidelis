using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class PuntoVenditaConfiguration : IEntityTypeConfiguration<PuntoVendita>
{
    public void Configure(EntityTypeBuilder<PuntoVendita> builder)
    {
        builder.ToTable("PuntiVendita");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Codice)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(p => p.Codice)
            .IsUnique();

        builder.Property(p => p.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Indirizzo)
            .HasMaxLength(300);

        builder.Property(p => p.Citta)
            .HasMaxLength(100);

        builder.Property(p => p.CAP)
            .HasMaxLength(10);

        builder.Property(p => p.Provincia)
            .HasMaxLength(50);

        builder.Property(p => p.Telefono)
            .HasMaxLength(20);

        builder.Property(p => p.Email)
            .HasMaxLength(255);

        builder.Property(p => p.PuntiPerEuro)
            .HasPrecision(5, 2)
            .HasDefaultValue(0.1m);

        builder.Property(p => p.OrariApertura)
            .HasMaxLength(1000); // JSON string

        builder.Property(p => p.RowVersion)
            .IsRowVersion();
    }
}
