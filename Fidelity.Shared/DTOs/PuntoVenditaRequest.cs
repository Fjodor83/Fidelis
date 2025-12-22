// Fidelity.Shared/DTOs/PuntoVenditaRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class PuntoVenditaRequest
    {
        [Required(ErrorMessage = "Codice punto vendita obbligatorio")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Il codice deve essere tra 2 e 10 caratteri")]
        public string Codice { get; set; }

        [Required(ErrorMessage = "Nome punto vendita obbligatorio")]
        [StringLength(100, ErrorMessage = "Il nome non può superare 100 caratteri")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Città obbligatoria")]
        [StringLength(50, ErrorMessage = "La città non può superare 50 caratteri")]
        public string Citta { get; set; }

        [Required(ErrorMessage = "Indirizzo obbligatorio")]
        [StringLength(200, ErrorMessage = "L'indirizzo non può superare 200 caratteri")]
        public string Indirizzo { get; set; }

        [Phone(ErrorMessage = "Numero di telefono non valido")]
        [StringLength(20, ErrorMessage = "Il telefono non può superare 20 caratteri")]
        public string Telefono { get; set; }

        public bool Attivo { get; set; } = true;
    }
}
