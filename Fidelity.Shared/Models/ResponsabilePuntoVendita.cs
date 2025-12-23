// Fidelity.Shared/Models/ResponsabilePuntoVendita.cs
namespace Fidelity.Shared.Models
{
    /// <summary>
    /// Junction table for many-to-many relationship between Responsabile and PuntoVendita
    /// </summary>
    public class ResponsabilePuntoVendita
    {
        public int ResponsabileId { get; set; }
        public Responsabile Responsabile { get; set; }

        public int PuntoVenditaId { get; set; }
        public PuntoVendita PuntoVendita { get; set; }
    }
}
