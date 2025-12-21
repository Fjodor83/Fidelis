// Fidelity.Shared/DTOs/VerificaEmailResponse.cs
namespace Fidelity.Shared.DTOs
{
    public class VerificaEmailResponse
    {
        public bool Valida { get; set; }
        public string Messaggio { get; set; }
        public string Token { get; set; }
        public string LinkRegistrazione { get; set; }
        public bool EmailInviata { get; set; }
    }
}