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
        
        builder.Property(t => t.Importo)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(t => t.Note)
            .HasMaxLength(500);
        
        // Indexes for performance
        builder.HasIndex(t => t.ClienteId);
        builder.HasIndex(t => t.DataTransazione);
        builder.HasIndex(t => new { t.ClienteId, t.DataTransazione });
    }
}
