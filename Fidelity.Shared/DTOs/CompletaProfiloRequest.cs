// Fidelity.Shared/DTOs/CompletaProfiloRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class CompletaProfiloRequest
    {
        [Required(ErrorMessage = "Nome obbligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Il nome deve essere tra 2 e 50 caratteri")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Cognome obbligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Il cognome deve essere tra 2 e 50 caratteri")]
        public string Cognome { get; set; }
    }
}
