// Fidelity.Shared/DTOs/ClienteDettaglioResponse.cs
using System;
using System.Collections.Generic;

namespace Fidelity.Shared.DTOs
{
    public class ClienteDettaglioResponse
    {
        public int Id { get; set; }
        public string CodiceFidelity { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public int PuntiTotali { get; set; }
        public DateTime DataRegistrazione { get; set; }
        public string PuntoVenditaRegistrazione { get; set; }
        public bool Attivo { get; set; }
        public List<TransazioneResponse> UltimeTransazioni { get; set; } = new();
    }
}