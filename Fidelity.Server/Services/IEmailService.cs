// Fidelity.Server/Services/IEmailService.cs
using System.Threading.Tasks;

namespace Fidelity.Server.Services
{
    public interface IEmailService
    {
        Task<(bool Success, string ErrorMessage)> InviaEmailVerificaAsync(string email, string nomeCliente, string token, string linkRegistrazione, string nomePuntoVendita);
        Task<(bool Success, string ErrorMessage)> InviaEmailBenvenutoAsync(string email, string nome, string codiceFidelity, byte[] cardDigitale);
        Task<(bool Success, string ErrorMessage)> InviaEmailPuntiGuadagnatiAsync(string email, string nome, int puntiGuadagnati, int nuovoSaldo, string nomePuntoVendita);
        Task<(bool Success, string ErrorMessage)> InviaEmailNuovoCouponAsync(string email, string nome, string titoloCoupon, string codiceCoupon, DateTime dataScadenza);
    }
}
