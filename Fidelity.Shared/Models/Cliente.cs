// Fidelity.Shared/Models/Cliente.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required, StringLength(12)]
        public string CodiceFidelity { get; set; } // Es: SUN123456789

        [Required, StringLength(100)]
        public string Nome { get; set; }

        [Required, StringLength(100)]
        public string Cognome { get; set; }

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; }

        [Phone, StringLength(20)]
        public string Telefono { get; set; }

        public DateTime DataRegistrazione { get; set; }

        // Tracciamento punto vendita di origine - Opzionale per registrazioni online
        public int? PuntoVenditaRegistrazioneId { get; set; }
        public PuntoVendita? PuntoVenditaRegistrazione { get; set; }

        // Responsabile che ha effettuato la registrazione - Opzionale per registrazioni online
        public int? ResponsabileRegistrazioneId { get; set; }
        public Responsabile? ResponsabileRegistrazione { get; set; }

        public int PuntiTotali { get; set; } = 0;

        public bool Attivo { get; set; } = true;

        public bool PrivacyAccettata { get; set; }

        // Link a transazioni
        // Password
        public string? PasswordHash { get; set; }
        
        // Reset password
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        public ICollection<Transazione> Transazioni { get; set; }
    }
}
