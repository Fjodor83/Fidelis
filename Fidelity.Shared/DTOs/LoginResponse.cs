// Fidelity.Shared/DTOs/LoginResponse.cs
namespace Fidelity.Shared.DTOs
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public int ResponsabileId { get; set; }
        public string Username { get; set; }
        public string NomeCompleto { get; set; }
        public string Ruolo { get; set; }
        public int? PuntoVenditaId { get; set; }
        public string PuntoVenditaCodice { get; set; }
        public string PuntoVenditaNome { get; set; }
        public bool RichiestaResetPassword { get; set; }
        public string Messaggio { get; set; }
    }
}