using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class CouponAssegnatoConfiguration : IEntityTypeConfiguration<CouponAssegnato>
{
    public void Configure(EntityTypeBuilder<CouponAssegnato> builder)
    {
        builder.ToTable("CouponAssegnati");

        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.AssegnatoDa)
            .HasMaxLength(100);

        builder.Property(ca => ca.Motivo)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(MotivoAssegnazione.Manuale)
            .HasSentinel((MotivoAssegnazione)(-1));

        // Relationships
        builder.HasOne(ca => ca.Coupon)
            .WithMany(c => c.CouponAssegnati)
            .HasForeignKey(ca => ca.CouponId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ca => ca.Cliente)
            .WithMany(c => c.CouponAssegnati)
            .HasForeignKey(ca => ca.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(ca => ca.ResponsabileUtilizzo)
            .WithMany()
            .HasForeignKey(ca => ca.ResponsabileUtilizzoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ca => ca.PuntoVenditaUtilizzo)
            .WithMany()
            .HasForeignKey(ca => ca.PuntoVenditaUtilizzoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ca => new { ca.CouponId, ca.ClienteId });
        builder.HasIndex(ca => new { ca.ClienteId, ca.Utilizzato });
    }
}
