using Fidelity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fidelity.Infrastructure.Persistence.Configurations;

public class ResponsabileConfiguration : IEntityTypeConfiguration<Responsabile>
{
    public void Configure(EntityTypeBuilder<Responsabile> builder)
    {
        builder.ToTable("Responsabili");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(r => r.Username)
            .IsUnique();

        builder.Property(r => r.NomeCompleto)
            .HasMaxLength(200);

        builder.Property(r => r.Email)
            .HasMaxLength(255);

        builder.Property(r => r.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(r => r.Ruolo)
            .HasMaxLength(20)
            .HasDefaultValue("Responsabile");

        builder.Property(r => r.UltimoAccessoIP)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(r => r.RowVersion)
            .IsRowVersion();
    }
}
