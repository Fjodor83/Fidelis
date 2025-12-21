// Fidelity.Server/Services/EmailService.cs
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Fidelity.Server.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> InviaEmailVerificaAsync(string email, string nomeCliente, string token, string linkRegistrazione, string nomePuntoVendita)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Suns - Zero&Company", _configuration["Email:From"]));
                message.To.Add(new MailboxAddress(nomeCliente, email));
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
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 40px 30px; }}
                            .content h2 {{ color: #333; margin-top: 0; }}
                            .content p {{ color: #666; line-height: 1.6; font-size: 16px; }}
                            .button {{ display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
                            .info-box {{ background-color: #f8f9fa; border-left: 4px solid #667eea; padding: 15px; margin: 20px 0; }}
                            .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #999; font-size: 14px; }}
                            .token {{ font-size: 24px; font-weight: bold; color: #667eea; letter-spacing: 2px; }}
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
                                <p>Sei stato registrato presso il nostro punto vendita <strong>{nomePuntoVendita}</strong>.</p>
                                <p>Per completare la tua registrazione e ricevere la tua Suns Fidelity Card digitale, clicca sul pulsante qui sotto:</p>
                                
                                <div style='text-align: center;'>
                                    <a href='{linkRegistrazione}' class='button'>COMPLETA REGISTRAZIONE</a>
                                </div>

                                <div class='info-box'>
                                    <p style='margin: 0;'><strong>⏰ Attenzione:</strong> Questo link è valido per <strong>15 minuti</strong>.</p>
                                    <p style='margin: 10px 0 0 0;'>Se non riesci a cliccare il pulsante, copia questo link nel tuo browser:</p>
                                    <p style='word-break: break-all; color: #667eea; margin: 10px 0 0 0;'>{linkRegistrazione}</p>
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

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _configuration["Email:SmtpServer"],
                    int.Parse(_configuration["Email:SmtpPort"]),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore invio email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InviaEmailBenvenutoAsync(string email, string nome, string codiceFidelity, byte[] cardDigitale)
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
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; }}
                            .header h1 {{ margin: 0; font-size: 28px; }}
                            .content {{ padding: 40px 30px; }}
                            .content h2 {{ color: #333; margin-top: 0; }}
                            .content p {{ color: #666; line-height: 1.6; font-size: 16px; }}
                            .code-box {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px; margin: 30px 0; }}
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

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _configuration["Email:SmtpServer"],
                    int.Parse(_configuration["Email:SmtpPort"]),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore invio email benvenuto: {ex.Message}");
                return false;
            }
        }
    }
}