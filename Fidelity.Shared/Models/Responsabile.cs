// Fidelity.Shared/Models/Responsabile.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.Models
{
    public class Responsabile
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string NomeCompleto { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public int? PuntoVenditaId { get; set; }
        public PuntoVendita PuntoVendita { get; set; }

        [Required, StringLength(20)]
        public string Ruolo { get; set; } // "Admin" o "Responsabile"

        public bool Attivo { get; set; } = true;

        public bool RichiestaResetPassword { get; set; } = false;

        public DateTime? UltimoAccesso { get; set; }
    }
}