using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Server.Services;
using Fidelity.Shared.DTOs;
using Fidelity.Domain.Entities;
using AutoMapper;
using Fidelity.Application.Common.Interfaces;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrazioneController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ICardGeneratorService _cardGenerator;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public RegistrazioneController(
            ApplicationDbContext context,
            IEmailService emailService,
            ICardGeneratorService cardGenerator,
            IConfiguration configuration,
            IMapper mapper)
        {
            _context = context;
            _emailService = emailService;
            _cardGenerator = cardGenerator;
            _configuration = configuration;
            _mapper = mapper;
        }

        /// <summary>
        /// Verifica disponibilità email e genera token di registrazione
        /// SOLO accessibile da responsabile autenticato
        /// </summary>
        [HttpPost("verifica-email")]
        [Authorize(Roles = "Responsabile,Admin")]
        public async Task<ActionResult<VerificaEmailResponse>> VerificaEmail([FromBody] VerificaEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var puntoVenditaId = int.Parse(User.FindFirst("PuntoVenditaId")?.Value ?? "0");

                // Verifica che il punto vendita nel request corrisponda a quello del responsabile
                if (request.PuntoVenditaId != puntoVenditaId && User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
                {
                    return Unauthorized(new VerificaEmailResponse
                    {
                        Valida = false,
                        Messaggio = "Non puoi registrare clienti per altri punti vendita."
                    });
                }

                // Verifica se email già registrata
                var emailEsistente = await _context.Clienti
                    .AnyAsync(c => c.Email == request.Email);

                if (emailEsistente)
                {
                    return BadRequest(new VerificaEmailResponse
                    {
                        Valida = false,
                        Messaggio = "Questa email è già registrata nel sistema."
                    });
                }

                // Verifica token esistente non scaduto per questa email
                var tokenEsistente = await _context.TokenRegistrazione
                    .Where(t => t.Email == request.Email
                        && !t.Utilizzato
                        && t.DataScadenza > DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                if (tokenEsistente != null)
                {
                    var linkEsistente = $"{_configuration["AppUrl"]}/registrazione/{tokenEsistente.Token}";
                    return Ok(new VerificaEmailResponse
                    {
                        Valida = true,
                        Token = tokenEsistente.Token,
                        LinkRegistrazione = linkEsistente,
                        Messaggio = "Un link di registrazione è già stato inviato a questa email.",
                        EmailInviata = false
                    });
                }

                // Genera nuovo token tramite domain entity factory
                var nuovoToken = TokenRegistrazione.Create(request.Email, request.PuntoVenditaId, responsabileId);

                _context.TokenRegistrazione.Add(nuovoToken);
                await _context.SaveChangesAsync();

                // Genera link registrazione
                var linkRegistrazione = $"{_configuration["AppUrl"]}/registrazione/{nuovoToken.Token}";

                // Ottieni info punto vendita per email
                var puntoVendita = await _context.PuntiVendita
                    .FirstOrDefaultAsync(p => p.Id == request.PuntoVenditaId);

                // Invia email
                var (emailInviata, queryError) = await _emailService.InviaEmailVerificaAsync(
                    request.Email,
                    "Cliente", // Nome generico, sarà inserito dopo
                    nuovoToken.Token,
                    linkRegistrazione,
                    puntoVendita?.Nome ?? "Suns"
                );

                return Ok(new VerificaEmailResponse
                {
                    Valida = true,
                    Token = nuovoToken.Token,
                    LinkRegistrazione = linkRegistrazione,
                    Messaggio = emailInviata
                        ? "Email di verifica inviata con successo."
                        : $"Token generato ma errore email: {queryError}",
                    EmailInviata = emailInviata
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new VerificaEmailResponse
                {
                    Valida = false,
                    Messaggio = "Errore durante la verifica email.",
                    EmailInviata = false
                });
            }
        }

        /// <summary>
        /// Valida il token di registrazione (chiamato quando il cliente clicca il link)
        /// Endpoint PUBBLICO
        /// </summary>
        [HttpGet("valida-token/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidaToken(string token)
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

                if (tokenRecord.IsScaduto())
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
            catch (Exception)
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

                if (tokenRecord == null || tokenRecord.Utilizzato || tokenRecord.IsScaduto())
                {
                    return BadRequest(new { success = false, messaggio = "Token non valido o scaduto." });
                }

                // Genera codice fidelity univoco
                var codiceFidelity = await GeneraCodiceFidelityUnivocoAsync();

                // Crea cliente tramite entity
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
                    PrivacyAccettataData = request.PrivacyAccettata ? DateTime.UtcNow : null,
                    Attivo = true,
                    PuntiTotali = 0
                };

                _context.Clienti.Add(nuovoCliente);

                // Marca token come utilizzato via domain method
                tokenRecord.SegnaUtilizzato();

                await _context.SaveChangesAsync();

                // Ricarica cliente con relazioni
                nuovoCliente = await _context.Clienti
                    .Include(c => c.PuntoVenditaRegistrazione)
                    .FirstAsync(c => c.Id == nuovoCliente.Id);

                // Genera card digitale
                byte[] cardDigitale = await _cardGenerator.GeneraCardDigitaleAsync(
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
                    cliente = _mapper.Map<ClienteResponse>(nuovoCliente)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, messaggio = "Errore durante la registrazione.", errore = ex.Message });
            }
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
