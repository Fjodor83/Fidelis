// Fidelity.Shared/DTOs/CompletaRegistrazioneRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class CompletaRegistrazioneRequest
    {
        [Required(ErrorMessage = "Token richiesto")]
        [StringLength(16, MinimumLength = 16)]
        public string Token { get; set; }

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [StringLength(100, MinimumLength = 2)]
        public string Cognome { get; set; }

        [Required(ErrorMessage = "Il telefono è obbligatorio")]
        [Phone(ErrorMessage = "Formato telefono non valido")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Devi accettare i termini e condizioni")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Devi accettare la privacy")]
        public bool PrivacyAccettata { get; set; }
    }
}