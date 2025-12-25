// Fidelity.Shared/DTOs/TransazioneResponse.cs
using System;

namespace Fidelity.Shared.DTOs
{
    public class TransazioneResponse
    {
        public int Id { get; set; }
        public string ClienteNome { get; set; }
        public string CodiceFidelity { get; set; }
        public int PuntiAssegnati { get; set; }
        public decimal? ImportoSpesa { get; set; }
        public DateTime DataTransazione { get; set; }
        public string PuntoVenditaNome { get; set; }
        public string ResponsabileNome { get; set; }
        public string TipoTransazione { get; set; }
        public string Note { get; set; }
    }
}