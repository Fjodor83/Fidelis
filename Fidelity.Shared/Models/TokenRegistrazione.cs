// Fidelity.Shared/Models/TokenRegistrazione.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Fidelity.Shared.Models
{
    public class TokenRegistrazione
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(16)]
        public string Token { get; set; } // 16 caratteri alfanumerici

        [Required]
        public int PuntoVenditaId { get; set; }
        public PuntoVendita PuntoVendita { get; set; }

        [Required]
        public int ResponsabileId { get; set; }
        public Responsabile Responsabile { get; set; }

        public DateTime DataCreazione { get; set; }
        public DateTime DataScadenza { get; set; } // +15 minuti

        public bool Utilizzato { get; set; } = false;
        public DateTime? DataUtilizzo { get; set; }
    }
}
