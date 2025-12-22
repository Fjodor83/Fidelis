// Fidelity.Server/Services/IEmailService.cs
using System.Threading.Tasks;

namespace Fidelity.Server.Services
{
    public interface IEmailService
    {
        Task<(bool Success, string ErrorMessage)> InviaEmailVerificaAsync(string email, string nomeCliente, string token, string linkRegistrazione, string nomePuntoVendita);
        Task<(bool Success, string ErrorMessage)> InviaEmailBenvenutoAsync(string email, string nome, string codiceFidelity, byte[] cardDigitale);
    }
}
