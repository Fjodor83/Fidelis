// Fidelity.Shared/DTOs/PuntoVenditaResponse.cs
namespace Fidelity.Shared.DTOs
{
    public class PuntoVenditaResponse
    {
        public int Id { get; set; }
        public string Codice { get; set; }
        public string Nome { get; set; }
        public string Citta { get; set; }
        public string Indirizzo { get; set; }
        public string Telefono { get; set; }
        public bool Attivo { get; set; }
        public int NumeroClienti { get; set; }
        public DateTime DataCreazione { get; set; }
    }
}
