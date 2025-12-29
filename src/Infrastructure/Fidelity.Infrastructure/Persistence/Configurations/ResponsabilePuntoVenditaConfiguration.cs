using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class ResponsabilePuntoVenditaConfiguration : IEntityTypeConfiguration<ResponsabilePuntoVendita>
{
    public void Configure(EntityTypeBuilder<ResponsabilePuntoVendita> builder)
    {
        builder.ToTable("ResponsabilePuntiVendita");

        builder.HasKey(rp => new { rp.ResponsabileId, rp.PuntoVenditaId });

        builder.HasOne(rp => rp.Responsabile)
            .WithMany(r => r.ResponsabilePuntiVendita)
            .HasForeignKey(rp => rp.ResponsabileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.PuntoVendita)
            .WithMany(pv => pv.ResponsabilePuntiVendita)
            .HasForeignKey(rp => rp.PuntoVenditaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
