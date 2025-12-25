// Fidelity.Shared/DTOs/AssegnaPuntiRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class AssegnaPuntiRequest
    {
        [Required(ErrorMessage = "Codice fedeltà obbligatorio")]
        public string CodiceFidelity { get; set; }

        [Required(ErrorMessage = "Importo spesa obbligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "Importo deve essere maggiore di 0")]
        public decimal ImportoSpesa { get; set; }

        public string Note { get; set; }
    }
}