// Fidelity.Shared/DTOs/CambiaPasswordRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class CambiaPasswordRequest
    {
        [Required(ErrorMessage = "Password attuale obbligatoria")]
        public string PasswordAttuale { get; set; }

        [Required(ErrorMessage = "Nuova password obbligatoria")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve essere di almeno 8 caratteri")]
        public string NuovaPassword { get; set; }

        [Required(ErrorMessage = "Conferma password obbligatoria")]
        [Compare("NuovaPassword", ErrorMessage = "Le password non corrispondono")]
        public string ConfermaPassword { get; set; }
    }
}