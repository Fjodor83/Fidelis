// Fidelity.Shared/DTOs/LoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username obbligatorio")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password obbligatoria")]
        public string Password { get; set; }
    }
}