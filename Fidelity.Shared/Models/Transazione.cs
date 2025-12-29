// Fidelity.Shared/Models/Transazione.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace Fidelity.Shared.Models
{
    public class Transazione
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int PuntoVenditaId { get; set; }
        public PuntoVendita PuntoVendita { get; set; }

        public int PuntiAssegnati { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ImportoSpesa { get; set; }

        public DateTime DataTransazione { get; set; }

        public string Note { get; set; }

        public int ResponsabileId { get; set; }
        public Responsabile Responsabile { get; set; }

        public string TipoTransazione { get; set; } // "Accumulo", "Riscatto", "Rettifica"
    }
}
