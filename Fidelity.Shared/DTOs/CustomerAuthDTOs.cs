using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class LoginClienteRequest
    {
        [Required]
        public string EmailOrCode { get; set; } // Can be Email or FidelityCode

        [Required]
        public string Password { get; set; }
    }

    public class LoginClienteResponse
    {
        public bool Success { get; set; }
        public string Messaggio { get; set; }
        public string Token { get; set; }
        public string? RefreshToken { get; set; }
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string? Email { get; set; }
        public string CodiceFidelity { get; set; }
        public int PuntiTotali { get; set; }
    }

    public class RegisterClienteRequest
    {
        // Step 1: Identification (New or Existing)
        public bool HasExistingCard { get; set; }
        public string? ExistingFidelityCode { get; set; }

        // Step 2: Personal Info (Required for New, auto-filled or verified for Existing)
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Cognome { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        public string? Telefono { get; set; }
        
        // Step 3: Security
        [Required, MinLength(6)]
        public string Password { get; set; }
        
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public bool PrivacyAccepted { get; set; }
        
        
        // Optional: auto-detect store if registering from a specific link, otherwise default
        [Required(ErrorMessage = "Seleziona il tuo punto vendita preferito")]
        public int? PuntoVenditaId { get; set; }
    }
}
