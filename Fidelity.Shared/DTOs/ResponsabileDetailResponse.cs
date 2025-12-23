// Fidelity.Shared/DTOs/ResponsabileDetailResponse.cs
namespace Fidelity.Shared.DTOs
{
    public class ResponsabileDetailResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Ruolo { get; set; }
        public bool Attivo { get; set; }
        public bool RichiestaResetPassword { get; set; }
        public DateTime? UltimoAccesso { get; set; }
        public List<PuntoVenditaBasicInfo> PuntiVendita { get; set; } = new();
    }

    public class PuntoVenditaBasicInfo
    {
        public int Id { get; set; }
        public string Codice { get; set; }
        public string Nome { get; set; }
    }
}
