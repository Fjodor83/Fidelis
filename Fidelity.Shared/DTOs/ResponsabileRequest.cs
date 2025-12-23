// Fidelity.Shared/DTOs/ResponsabileRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class ResponsabileRequest
    {
        [Required(ErrorMessage = "Username obbligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "L'username deve essere tra 3 e 50 caratteri")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Nome completo obbligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Il nome completo deve essere tra 3 e 100 caratteri")]
        public string NomeCompleto { get; set; }

        [Required(ErrorMessage = "Email obbligatoria")]
        [EmailAddress(ErrorMessage = "Email non valida")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Seleziona almeno un punto vendita")]
        [MinLength(1, ErrorMessage = "Seleziona almeno un punto vendita")]
        public List<int> PuntiVenditaIds { get; set; } = new();

        [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve essere di almeno 8 caratteri")]
        public string? Password { get; set; }

        public bool RichiestaResetPassword { get; set; } = true;

        public bool Attivo { get; set; } = true;
    }
}
