// Fidelity.Shared/Models/PuntoVendita.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.Models
{
    public class PuntoVendita
    {
        public int Id { get; set; }

        [Required, StringLength(4)]
        public string Codice { get; set; } // Es: SU01 (Suns 01)

        [Required, StringLength(200)]
        public string Nome { get; set; }

        [StringLength(500)]
        public string Indirizzo { get; set; }

        [StringLength(50)]
        public string Citta { get; set; }

        [Phone]
        public string Telefono { get; set; }

        public bool Attivo { get; set; } = true;

        // Relazioni
        public ICollection<Responsabile> Responsabili { get; set; }
        public ICollection<Cliente> ClientiRegistrati { get; set; }
        public ICollection<Transazione> Transazioni { get; set; }
    }
}