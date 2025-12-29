using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class CouponDTO
    {
        public int Id { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Titolo { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public decimal ValoreSconto { get; set; }
        public string TipoSconto { get; set; } = "Percentuale";
        public DateTime DataInizio { get; set; }
        public DateTime DataScadenza { get; set; }
        public bool Attivo { get; set; }
        public decimal? ImportoMinimoOrdine { get; set; }
        public int? LimiteUtilizzoGlobale { get; set; }
        public int UtilizziTotali { get; set; }
    }

    public class CouponRequest
    {
        [Required(ErrorMessage = "Il codice è obbligatorio")]
        [StringLength(20, ErrorMessage = "Il codice non può superare i 20 caratteri")]
        public string Codice { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il titolo è obbligatorio")]
        [StringLength(200, ErrorMessage = "Il titolo non può superare i 200 caratteri")]
        public string Titolo { get; set; } = string.Empty;

        public string Descrizione { get; set; } = string.Empty;

        [Range(0.01, 999.99, ErrorMessage = "Il valore deve essere maggiore di 0")]
        public decimal ValoreSconto { get; set; }

        public string TipoSconto { get; set; } = "Percentuale";

        public DateTime DataInizio { get; set; } = DateTime.UtcNow;

        public DateTime DataScadenza { get; set; } = DateTime.UtcNow.AddDays(30);

        public bool Attivo { get; set; } = true;

        public decimal? ImportoMinimoOrdine { get; set; }
        public int? LimiteUtilizzoPerCliente { get; set; }
        public int? LimiteUtilizzoGlobale { get; set; }
    }

    public class AssegnaCouponRequest
    {
        [Required]
        public int CouponId { get; set; }

        [Required]
        public int ClienteId { get; set; }
    }
}
