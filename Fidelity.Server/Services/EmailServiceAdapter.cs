using System;
using System.Threading.Tasks;

namespace Fidelity.Server.Services
{
    /// <summary>
    /// Adapter that wraps the Server EmailService to implement the Application IEmailService interface
    /// </summary>
    public class EmailServiceAdapter : Fidelity.Application.Common.Interfaces.IEmailService
    {
        private readonly EmailService _emailService;

        public EmailServiceAdapter(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<(bool Success, string ErrorMessage)> InviaEmailVerificaAsync(
            string email,
            string nomeCliente,
            string token,
            string linkRegistrazione,
            string nomePuntoVendita)
        {
            return await _emailService.InviaEmailVerificaAsync(
                email,
                nomeCliente,
                token,
                linkRegistrazione,
                nomePuntoVendita);
        }

        public async Task<(bool Success, string ErrorMessage)> InviaEmailBenvenutoAsync(
            string email,
            string nome,
            string codiceFidelity,
            byte[] cardDigitale)
        {
            return await _emailService.InviaEmailBenvenutoAsync(
                email,
                nome,
                codiceFidelity,
                cardDigitale);
        }

        public async Task<(bool Success, string ErrorMessage)> InviaEmailPuntiGuadagnatiAsync(
            string email,
            string nome,
            int puntiGuadagnati,
            int nuovoSaldo,
            string nomePuntoVendita)
        {
            return await _emailService.InviaEmailPuntiGuadagnatiAsync(
                email,
                nome,
                puntiGuadagnati,
                nuovoSaldo,
                nomePuntoVendita);
        }

        public async Task<(bool Success, string ErrorMessage)> InviaEmailNuovoCouponAsync(
            string email,
            string nome,
            string titoloCoupon,
            string codiceCoupon,
            DateTime dataScadenza)
        {
            return await _emailService.InviaEmailNuovoCouponAsync(
                email,
                nome,
                titoloCoupon,
                codiceCoupon,
                dataScadenza);
        }
    }
}