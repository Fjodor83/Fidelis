using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class TokenRegistrazioneConfiguration : IEntityTypeConfiguration<TokenRegistrazione>
{
    public void Configure(EntityTypeBuilder<TokenRegistrazione> builder)
    {
        builder.ToTable("TokenRegistrazione");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Token)
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(t => t.Token)
            .IsUnique();

        // Relationships
        builder.HasOne(t => t.PuntoVendita)
            .WithMany()
            .HasForeignKey(t => t.PuntoVenditaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(t => t.Responsabile)
            .WithMany(r => r.TokenRegistrazioniCreati)
            .HasForeignKey(t => t.ResponsabileId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(t => t.Email);
        builder.HasIndex(t => new { t.Email, t.Utilizzato });
    }
}
