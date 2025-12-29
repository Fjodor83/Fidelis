using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clienti");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CodiceFidelity)
            .HasMaxLength(12)
            .IsRequired();

        builder.HasIndex(c => c.CodiceFidelity)
            .IsUnique();

        builder.Property(c => c.Nome)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Cognome)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Telefono)
            .HasMaxLength(20);

        builder.Property(c => c.PasswordHash)
            .HasMaxLength(255);

        builder.Property(c => c.PasswordResetToken)
            .HasMaxLength(255);

        builder.Property(c => c.Livello)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(LivelloFedelta.Bronze);

        // Optimistic concurrency
        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasOne(c => c.PuntoVenditaRegistrazione)
            .WithMany(p => p.ClientiRegistrati)
            .HasForeignKey(c => c.PuntoVenditaRegistrazioneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ResponsabileRegistrazione)
            .WithMany()
            .HasForeignKey(c => c.ResponsabileRegistrazioneId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for search performance
        builder.HasIndex(c => c.Nome);
        builder.HasIndex(c => c.Cognome);
        builder.HasIndex(c => new { c.Attivo, c.IsDeleted });
    }
}
