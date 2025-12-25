using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fidelity.Shared.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Codice { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Titolo { get; set; } = string.Empty;

        [StringLength(500)]
        public string Descrizione { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal ValoreSconto { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoSconto { get; set; } = "Percentuale"; // "Percentuale" o "Fisso"

        public DateTime DataInizio { get; set; }

        public DateTime DataScadenza { get; set; }

        public bool Attivo { get; set; } = true;
    }
}
