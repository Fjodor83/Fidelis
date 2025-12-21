// Fidelity.Shared/DTOs/ClienteResponse.cs
using System;

namespace Fidelity.Shared.DTOs
{
    public class ClienteResponse
    {
        public int Id { get; set; }
        public string CodiceFidelity { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public int PuntiTotali { get; set; }
        public DateTime DataRegistrazione { get; set; }
        public string PuntoVenditaRegistrazione { get; set; }
        public string PuntoVenditaCodice { get; set; }
        public bool Attivo { get; set; }
    }
}