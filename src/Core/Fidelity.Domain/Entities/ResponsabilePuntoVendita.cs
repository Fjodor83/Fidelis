using Fidelity.Domain.Common;

namespace Fidelity.Domain.Entities;

/// <summary>
/// Many-to-many relationship between Responsabile and PuntoVendita
/// </summary>
public class ResponsabilePuntoVendita
{
    public int ResponsabileId { get; set; }
    public int PuntoVenditaId { get; set; }

    public DateTime DataAssociazione { get; set; } = DateTime.UtcNow;
    public bool Principale { get; set; } = false; // Primary store for this manager

    // Navigation properties
    public virtual Responsabile Responsabile { get; set; } = null!;
    public virtual PuntoVendita PuntoVendita { get; set; } = null!;
}
