using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using MailKit; 
using Fidelity.Application.Common.Interfaces;

namespace Fidelity.Server.Services
{
    public class EmailService : Application.Common.Interfaces.IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(bool Success, string? Error)> InviaEmailVerificaAsync(string email, string nome, string token, string linkRegistrazione, string puntoVenditaNome)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Suns - Zero&Company", _configuration["Email:From"]));
                message.To.Add(new MailboxAddress(nome, email));
                message.Subject = "🎁 Completa la tua registrazione Suns Fidelity Card";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
                            .header {{ background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); color: white; padding: 40px 20px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 40px 30px; }}
                            .content h2 {{ color: #333; margin-top: 0; }}
                            .content p {{ color: #666; line-height: 1.6; font-size: 16px; }}
                            .button {{ display: inline-block; background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
                            .info-box {{ background-color: #f8f9fa; border-left: 4px solid #105a12ff; padding: 15px; margin: 20px 0; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #999; font-size: 14px; }}
                            .token {{ font-size: 24px; font-weight: bold; color: #105a12ff; letter-spacing: 2px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>☀️ Suns - Zero&Company</h1>
                                <p style='margin: 10px 0 0 0;'>La tua Fidelity Card ti aspetta!</p>
                            </div>
                            <div class='content'>
                                <h2>Ciao! 👋</h2>
                                <p>Sei stato registrato presso il nostro punto vendita <strong>{puntoVenditaNome}</strong>.</p>
                                <p>Per completare la tua registrazione e ricevere la tua Suns Fidelity Card digitale, clicca sul pulsante qui sotto:</p>
                                
                                <div style='text-align: center; margin: 20px 0;'>
                                    <table border='0' cellpadding='0' cellspacing='0' style='margin: 0 auto;'>
                                        <tr>
                                            <td align='center' bgcolor='#105a12' style='background-color: #105a12; background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); border-radius: 5px; padding: 15px 40px;'>
                                                <a href='{linkRegistrazione}' style='color: #ffffff; font-family: Arial, sans-serif; font-size: 16px; font-weight: bold; text-decoration: none; display: inline-block;'>
                                                    <span style='color: #ffffff; text-decoration: none;'>COMPLETA REGISTRAZIONE</span>
                                                </a>
                                            </td>
                                        </tr>
                                    </table>
                                </div>

                                <div class='info-box'>
                                    <p style='margin: 0;'><strong>⏰ Attenzione:</strong> Questo link è valido per <strong>15 minuti</strong>.</p>
                                    <p style='margin: 10px 0 0 0;'>Se non riesci a cliccare il pulsante, copia questo link nel tuo browser:</p>
                                    <p style='word-break: break-all; color: #105a12ff; margin: 10px 0 0 0;'>{linkRegistrazione}</p>
                                </div>

                                <div class='info-box'>
                                    <p style='margin: 0;'><strong>🔑 Il tuo codice di verifica:</strong></p>
                                    <p class='token'>{token}</p>
                                </div>

                                <p>Dopo aver completato la registrazione, riceverai:</p>
                                <ul style='color: #666;'>
                                    <li>Il tuo codice Fidelity personale</li>
                                    <li>La tua card digitale con QR code</li>
                                    <li>Accesso a tutti i vantaggi del programma fedeltà</li>
                                </ul>
                            </div>
                            <div class='footer'>
                                <p>© 2024 Suns - Zero&Company. Tutti i diritti riservati.</p>
                                <p>Questa email è stata inviata perché ti sei registrato presso uno dei nostri punti vendita.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                // Use ProtocolLogger to log SMTP communication to Console
                using var client = new SmtpClient(new ProtocolLogger(Console.OpenStandardOutput()));
                
                await client.ConnectAsync(
                    _configuration["Email:SmtpServer"] ?? "smtp.gmail.com",
                    int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    _configuration["Email:Username"] ?? "",
                    _configuration["Email:Password"] ?? ""
                );

                await client.SendAsync(message);
                Console.WriteLine($"[EmailService] Email sent successfully to {email}");
                await client.DisconnectAsync(true);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore invio email: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<bool> InviaEmailBenvenutoAsync(string email, string nome, string codiceFidelity, byte[]? cardDigitale = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Suns - Zero&Company", _configuration["Email:From"]));
                message.To.Add(new MailboxAddress(nome, email));
                message.Subject = $"🎉 Benvenuto {nome}! La tua Suns Fidelity Card è pronta";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
                            .header {{ background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); color: white; padding: 40px 20px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 40px 30px; }}
                            .content h2 {{ color: #333; margin-top: 0; }}
                            .content p {{ color: #666; line-height: 1.6; font-size: 16px; }}
                            .code-box {{ background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); color: white; padding: 30px; text-align: center; border-radius: 10px; margin: 30px 0; }}
                            .code-box h2 {{ margin: 0 0 10px 0; font-size: 18px; }}
                            .code {{ font-size: 36px; font-weight: bold; letter-spacing: 3px; }}
                            .benefits {{ background-color: #f8f9fa; padding: 20px; border-radius: 10px; margin: 20px 0; }}
                            .benefits li {{ margin: 10px 0; color: #666; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #999; font-size: 14px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>☀️ Benvenuto in Suns!</h1>
                                <p style='margin: 10px 0 0 0;'>La tua Fidelity Card è attiva</p>
                            </div>
                            <div class='content'>
                                <h2>Ciao {nome}! 🎉</h2>
                                <p>La tua registrazione è stata completata con successo!</p>
                                
                                <div class='code-box'>
                                    <h2>Il tuo Codice Fidelity</h2>
                                    <div class='code'>{codiceFidelity}</div>
                                </div>

                                <p><strong>📱 La tua card digitale è allegata a questa email.</strong> Salvala sul tuo telefono e mostrarla ad ogni acquisto per accumulare punti!</p>

                                <div class='benefits'>
                                    <h3 style='color: #333; margin-top: 0;'>✨ I tuoi vantaggi:</h3>
                                    <ul>
                                        <li><strong>Accumula punti</strong> ad ogni acquisto</li>
                                        <li><strong>Sconti esclusivi</strong> riservati ai membri</li>
                                        <li><strong>Promozioni speciali</strong> in anteprima</li>
                                        <li><strong>Premi</strong> al raggiungimento di soglie punti</li>
                                    </ul>
                                </div>

                                <p style='text-align: center; margin-top: 30px;'>
                                    <strong>Non vediamo l'ora di rivederti nei nostri negozi!</strong>
                                </p>
                            </div>
                            <div class='footer'>
                                <p>© 2024 Suns - Zero&Company. Tutti i diritti riservati.</p>
                                <p>Per assistenza: info@sunscompany.com</p>
                            </div>
                        </div>
                    </body>
                    </html>"
                };

                // Allega la card digitale
                if (cardDigitale != null && cardDigitale.Length > 0)
                {
                    bodyBuilder.Attachments.Add($"SunsFidelityCard_{codiceFidelity}.png", cardDigitale, ContentType.Parse("image/png"));
                }

                message.Body = bodyBuilder.ToMessageBody();

                // Use ProtocolLogger to log SMTP communication to Console
                using var client = new SmtpClient(new ProtocolLogger(Console.OpenStandardOutput()));
                
                await client.ConnectAsync(
                    _configuration["Email:SmtpServer"] ?? "smtp.gmail.com",
                    int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    _configuration["Email:Username"] ?? "",
                    _configuration["Email:Password"] ?? ""
                );

                await client.SendAsync(message);
                Console.WriteLine($"[EmailService] Welcome email sent successfully to {email}");
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore invio email benvenuto: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InviaEmailPuntiAssegnatiAsync(string email, string nome, int puntiAssegnati, int puntiTotali, decimal importoSpesa)
        {
            try
            {
                var nomePuntoVendita = "Suns Store"; 
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Suns - Zero&Company", _configuration["Email:From"]));
                message.To.Add(new MailboxAddress(nome, email));
                message.Subject = $"🌟 Hai guadagnato {puntiAssegnati} punti!";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
                            .header {{ background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); color: white; padding: 40px 20px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 40px 30px; }}
                            .points-box {{ background-color: #fff3cd; border: 1px solid #ffeeba; color: #856404; padding: 20px; text-align: center; border-radius: 10px; margin: 20px 0; }}
                            .points {{ font-size: 32px; font-weight: bold; color: #105a12ff; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #999; font-size: 14px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>☀️ Nuovi Punti per te!</h1>
                            </div>
                            <div class='content'>
                                <h2>Ciao {nome}! 👋</h2>
                                <p>Grazie per il tuo acquisto presso <strong>{nomePuntoVendita}</strong>.</p>
                                
                                <div class='points-box'>
                                    <p style='margin: 0;'>Hai guadagnato:</p>
                                    <div class='points'>+{puntiAssegnati} Punti</div>
                                </div>

                                <p style='text-align: center; font-size: 18px;'>
                                    Il tuo nuovo saldo totale è:<br>
                                    <strong>{puntiTotali} Punti</strong>
                                </p>

                                <p>Continua così per sbloccare premi esclusivi!</p>
                            </div>
                            <div class='footer'>
                                <p>© 2024 Suns - Zero&Company. Tutti i diritti riservati.</p>
                            </div>
                        </div>
                    </body>
                    </html>"
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient(new ProtocolLogger(Console.OpenStandardOutput()));
                await client.ConnectAsync(
                    _configuration["Email:SmtpServer"] ?? "smtp.gmail.com",
                    int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls
                );
                await client.AuthenticateAsync(
                    _configuration["Email:Username"] ?? "",
                    _configuration["Email:Password"] ?? ""
                );
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore invio email punti: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InviaEmailNuovoCouponAsync(string email, string nome, string titoloCoupon, string codiceCoupon, DateTime dataScadenza)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Suns - Zero&Company", _configuration["Email:From"]));
                message.To.Add(new MailboxAddress(nome, email));
                message.Subject = "🎁 Nuovo Coupon Disponibile!";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 30px auto; background-color: #ffffff; border-radius: 10px; overflow: hidden; box-shadow: 0 0 20px rgba(0,0,0,0.1); }}
                            .header {{ background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); color: white; padding: 40px 20px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 32px; }}
                            .header .emoji {{ font-size: 64px; }}
                            .content {{ padding: 40px 30px; }}
                            .content h2 {{ color: #105a12ff; margin-top: 0; }}
                            .content p {{ color: #666; line-height: 1.6; font-size: 16px; }}
                            .coupon-box {{ background: linear-gradient(135deg, #105a12ff 0%, #053e30ff 100%); color: white; padding: 30px; border-radius: 10px; text-align: center; margin: 30px 0; box-shadow: 0 4px 15px rgba(16, 90, 18, 0.3); }}
                            .coupon-code {{ font-size: 36px; font-weight: bold; letter-spacing: 4px; border: 3px dashed white; padding: 20px; border-radius: 10px; margin: 20px 0; }}
                            .validity {{ background-color: rgba(255,255,255,0.2); padding: 10px; border-radius: 5px; margin-top: 15px; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #999; font-size: 14px; }}
                            .highlight {{ color: #105a12ff; font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <div class='emoji'>🎁</div>
                                <h1>Nuovo Coupon!</h1>
                            </div>
                            <div class='content'>
                                <h2>Ciao {nome}! 🌟</h2>
                                <p>Abbiamo una sorpresa speciale per te! È disponibile un <strong>nuovo coupon</strong> che puoi utilizzare subito.</p>
                                
                                <div class='coupon-box'>
                                    <h3 style='margin:0;font-size:24px;'>{titoloCoupon}</h3>
                                    <div class='coupon-code'>{codiceCoupon}</div>
                                    <div class='validity'>
                                        ⏰ Valido fino al {dataScadenza:dd/MM/yyyy}
                                    </div>
                                </div>

                                <p><strong>Come utilizzarlo:</strong></p>
                                <ol style='color:#666;line-height:1.8;'>
                                    <li>Accedi al tuo account Suns Fidelity</li>
                                    <li>Vai alla sezione ""I Miei Coupon""</li>
                                    <li>Troverai il coupon pronto da utilizzare</li>
                                    <li>Mostra il codice in negozio per ottenere il tuo sconto!</li>
                                </ol>

                                <p style='margin-top:30px;'>Non perdere questa occasione! Il coupon è già stato aggiunto automaticamente al tuo account.</p>
                            </div>
                            <div class='footer'>
                                <p>Questo è un messaggio automatico dalla tua Suns Fidelity Card.</p>
                                <p style='margin-top:10px;'>© 2025 Suns - Zero&Company. Tutti i diritti riservati.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(
                        _configuration["Email:SmtpServer"] ?? "smtp.gmail.com",
                        int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                        SecureSocketOptions.StartTls
                    );
                    await client.AuthenticateAsync(
                        _configuration["Email:Username"] ?? "",
                        _configuration["Email:Password"] ?? ""
                    );
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> InviaEmailLivelloRaggiuntoAsync(string email, string nome, string nuovoLivello)
        {
            // TODO: Implementare template per livello raggiunto
            return true;
        }

        public async Task<bool> InviaEmailResetPasswordAsync(string email, string nome, string resetToken, string resetLink)
        {
            // TODO: Implementare template per reset password
            return true;
        }
    }
}