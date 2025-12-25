using System;

namespace Fidelity.Shared.DTOs
{
    public class CouponAssegnatoDTO
    {
        public int Id { get; set; }
        public string Codice { get; set; } = string.Empty;
        public string Titolo { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        public decimal ValoreSconto { get; set; }
        public string TipoSconto { get; set; } = "Percentuale";
        public DateTime DataAssegnazione { get; set; }
        public DateTime? DataUtilizzo { get; set; }
        public bool Utilizzato { get; set; }
        public DateTime DataScadenza { get; set; }
        public bool Scaduto => DateTime.UtcNow > DataScadenza;
    }

    public class RiscattaCouponRequest
    {
        public int CouponAssegnatoId { get; set; }
    }
}
