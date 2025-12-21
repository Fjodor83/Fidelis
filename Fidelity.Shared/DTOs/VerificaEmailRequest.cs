// Fidelity.Shared/DTOs/VerificaEmailRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class VerificaEmailRequest
    {
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Formato email non valido")]
        public string Email { get; set; }

        // Sempre valorizzato - registrazione SOLO da responsabile
        [Required]
        public int PuntoVenditaId { get; set; }
    }
}