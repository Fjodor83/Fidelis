using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fidelity.Server.Data;
using Fidelity.Server.Services;
using Fidelity.Shared.Models;
using Fidelity.Shared.DTOs; // Assuming DTOs namespace
using Microsoft.AspNetCore.Authorization;

namespace Fidelity.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrazioneController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ICardGeneratorService _cardGenerator;

        public RegistrazioneController(
            ApplicationDbContext context,
            IEmailService emailService,
            ICardGeneratorService cardGenerator)
        {
            _context = context;
            _emailService = emailService;
            _cardGenerator = cardGenerator;
        }

        [HttpGet("verifica/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerificaToken(string token)
        {
            try
            {
                var tokenRecord = await _context.TokenRegistrazione
                    .Include(t => t.PuntoVendita)
                    .FirstOrDefaultAsync(t => t.Token == token);

                if (tokenRecord == null)
                    return NotFound(new { valido = false, messaggio = "Token non trovato." });

                if (tokenRecord.Utilizzato)
                    return BadRequest(new { valido = false, messaggio = "Questo token è già stato utilizzato." });

                if (tokenRecord.DataScadenza < DateTime.UtcNow)
                    return BadRequest(new { valido = false, messaggio = "Token scaduto. Recati nuovamente in negozio." });

                return Ok(new
                {
                    valido = true,
                    email = tokenRecord.Email,
                    puntoVenditaId = tokenRecord.PuntoVenditaId,
                    puntoVenditaNome = tokenRecord.PuntoVendita?.Nome,
                    scadenza = tokenRecord.DataScadenza,
                    messaggio = "Token valido. Procedi con la registrazione."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { valido = false, messaggio = "Errore durante la validazione token." });
            }
        }

        /// <summary>
        /// Completa la registrazione del cliente con i dati personali
        /// Endpoint PUBBLICO
        /// </summary>
        [HttpPost("completa")]
        [AllowAnonymous]
        public async Task<ActionResult<ClienteResponse>> CompletaRegistrazione([FromBody] CompletaRegistrazioneRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Valida token
                var tokenRecord = await _context.TokenRegistrazione
                    .Include(t => t.PuntoVendita)
                    .FirstOrDefaultAsync(t => t.Token == request.Token);

                if (tokenRecord == null || tokenRecord.Utilizzato || tokenRecord.DataScadenza < DateTime.UtcNow)
                {
                    return BadRequest(new { success = false, messaggio = "Token non valido o scaduto." });
                }

                // Genera codice fidelity univoco
                var codiceFidelity = await GeneraCodiceFidelityUnivocoAsync();

                // Crea cliente
                var nuovoCliente = new Cliente
                {
                    CodiceFidelity = codiceFidelity,
                    Nome = request.Nome,
                    Cognome = request.Cognome,
                    Email = tokenRecord.Email,
                    Telefono = request.Telefono,
                    DataRegistrazione = DateTime.UtcNow,
                    PuntoVenditaRegistrazioneId = tokenRecord.PuntoVenditaId,
                    ResponsabileRegistrazioneId = tokenRecord.ResponsabileId,
                    PrivacyAccettata = request.PrivacyAccettata,
                    Attivo = true,
                    PuntiTotali = 0
                };

                _context.Clienti.Add(nuovoCliente);

                // Marca token come utilizzato
                tokenRecord.Utilizzato = true;
                tokenRecord.DataUtilizzo = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Ricarica cliente con relazioni
                nuovoCliente = await _context.Clienti
                    .Include(c => c.PuntoVenditaRegistrazione)
                    .FirstAsync(c => c.Id == nuovoCliente.Id);

                // Genera card digitale
                var cardDigitale = await _cardGenerator.GeneraCardDigitaleAsync(
                    nuovoCliente,
                    nuovoCliente.PuntoVenditaRegistrazione
                );

                // Invia email benvenuto con card
                await _emailService.InviaEmailBenvenutoAsync(
                    nuovoCliente.Email,
                    nuovoCliente.Nome,
                    nuovoCliente.CodiceFidelity,
                    cardDigitale
                );

                return Ok(new
                {
                    success = true,
                    messaggio = "Registrazione completata! Controlla la tua email per la card digitale.",
                    cliente = new ClienteResponse
                    {
                        Id = nuovoCliente.Id,
                        CodiceFidelity = nuovoCliente.CodiceFidelity,
                        NomeCompleto = $"{nuovoCliente.Nome} {nuovoCliente.Cognome}",
                        Email = nuovoCliente.Email,
                        Telefono = nuovoCliente.Telefono,
                        PuntiTotali = nuovoCliente.PuntiTotali,
                        DataRegistrazione = nuovoCliente.DataRegistrazione,
                        PuntoVenditaRegistrazione = nuovoCliente.PuntoVenditaRegistrazione.Nome,
                        PuntoVenditaCodice = nuovoCliente.PuntoVenditaRegistrazione.Codice
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, messaggio = "Errore durante la registrazione." });
            }
        }

        private string GeneraToken16Cifre()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<string> GeneraCodiceFidelityUnivocoAsync()
        {
            string codice;
            bool esiste;

            do
            {
                // Genera codice formato SUNxxxxxxxxx (12 caratteri totali)
                var numero = new Random().Next(100000000, 999999999);
                codice = $"SUN{numero}";

                esiste = await _context.Clienti.AnyAsync(c => c.CodiceFidelity == codice);
            }
            while (esiste);

            return codice;
        }
    }
}
