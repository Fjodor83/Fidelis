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
        
        builder.Property(c => c.Nome)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(c => c.Cognome)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(c => c.Email)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(c => c.Telefono)
            .HasMaxLength(20);
        
        // Indexes
        builder.HasIndex(c => c.CodiceFidelity)
            .IsUnique();
        
        builder.HasIndex(c => c.Email)
            .IsUnique();
        
        builder.HasIndex(c => c.DataRegistrazione);
    }
}
