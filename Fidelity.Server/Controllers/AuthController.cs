// Fidelity.Server/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fidelity.Server.Data;
using Fidelity.Shared.DTOs;
using Fidelity.Shared.Models;
using BCrypt.Net;
using Fidelity.Server.Services;
using Microsoft.AspNetCore.Authorization;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context, 
            IConfiguration configuration,
            IEmailService emailService,
            ICardGeneratorService cardGenerator)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _cardGenerator = cardGenerator;
        }

        private readonly IEmailService _emailService;
        private readonly ICardGeneratorService _cardGenerator;

        /// <summary>
        /// Login per responsabili punti vendita e admin
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var responsabile = await _context.Responsabili
                    .Include(r => r.ResponsabilePuntiVendita)
                        .ThenInclude(rp => rp.PuntoVendita)
                    .FirstOrDefaultAsync(r => r.Username == request.Username && r.Attivo);

                if (responsabile == null)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Messaggio = "Username o password non corretti."
                    });
                }

                // Verifica password con BCrypt
                if (!BCrypt.Net.BCrypt.Verify(request.Password, responsabile.PasswordHash))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Messaggio = "Username o password non corretti."
                    });
                }

                // Aggiorna ultimo accesso
                responsabile.UltimoAccesso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Ottieni il primo punto vendita associato (se esiste)
                var primoPuntoVendita = responsabile.ResponsabilePuntiVendita?.FirstOrDefault()?.PuntoVendita;

                // Genera JWT Token
                var token = GeneraJwtToken(responsabile, primoPuntoVendita?.Id);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Token = token,
                    ResponsabileId = responsabile.Id,
                    Username = responsabile.Username,
                    NomeCompleto = responsabile.NomeCompleto,
                    Ruolo = responsabile.Ruolo,
                    PuntoVenditaId = primoPuntoVendita?.Id,
                    PuntoVenditaCodice = primoPuntoVendita?.Codice,
                    PuntoVenditaNome = primoPuntoVendita?.Nome,
                    RichiestaResetPassword = responsabile.RichiestaResetPassword,
                    ProfiloIncompleto = string.IsNullOrWhiteSpace(responsabile.NomeCompleto),
                    Messaggio = "Login effettuato con successo."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Messaggio = "Errore durante il login."
                });
            }
        }

        /// <summary>
        /// Cambia password per responsabile autenticato
        /// </summary>
        [HttpPost("cambia-password")]
        [Authorize]
        public async Task<IActionResult> CambiaPassword([FromBody] CambiaPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var responsabile = await _context.Responsabili.FindAsync(responsabileId);

                if (responsabile == null)
                    return NotFound(new { success = false, messaggio = "Responsabile non trovato." });

                // Verifica password attuale
                if (!BCrypt.Net.BCrypt.Verify(request.PasswordAttuale, responsabile.PasswordHash))
                {
                    return BadRequest(new { success = false, messaggio = "Password attuale non corretta." });
                }

                // Hash nuova password
                responsabile.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuovaPassword);
                responsabile.RichiestaResetPassword = false;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, messaggio = "Password cambiata con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, messaggio = "Errore durante il cambio password." });
            }
        }

        /// <summary>
        /// Completa profilo responsabile con nome e cognome reali
        /// </summary>
        [HttpPost("completa-profilo")]
        [Authorize]
        public async Task<IActionResult> CompletaProfilo([FromBody] CompletaProfiloRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var responsabile = await _context.Responsabili.FindAsync(responsabileId);

                if (responsabile == null)
                    return NotFound(new { success = false, messaggio = "Responsabile non trovato." });

                // Valida che il profilo sia incompleto (NomeCompleto vuoto)
                if (!string.IsNullOrWhiteSpace(responsabile.NomeCompleto))
                    return BadRequest(new { success = false, messaggio = "Profilo già completato." });

                // Aggiorna con nome reale
                responsabile.NomeCompleto = $"{request.Nome} {request.Cognome}";
                await _context.SaveChangesAsync();

                return Ok(new { success = true, messaggio = "Profilo completato con successo.", nomeCompleto = responsabile.NomeCompleto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, messaggio = "Errore durante il completamento del profilo." });
            }
        }

        private string GeneraJwtToken(Responsabile responsabile, int? puntoVenditaId)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, responsabile.Id.ToString()),
                new Claim(ClaimTypes.Name, responsabile.Username),
                new Claim(ClaimTypes.Role, responsabile.Ruolo),
                new Claim("PuntoVenditaId", puntoVenditaId?.ToString() ?? "0"),
                new Claim("NomeCompleto", responsabile.NomeCompleto ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Login per clienti finali
        /// </summary>
        [HttpPost("login/cliente")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginClienteResponse>> LoginCliente([FromBody] LoginClienteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Cerca per Email o Codice
                var cliente = await _context.Clienti
                    .FirstOrDefaultAsync(c => (c.Email == request.EmailOrCode || c.CodiceFidelity == request.EmailOrCode) && c.Attivo);

                if (cliente == null)
                {
                    return Unauthorized(new LoginClienteResponse
                    {
                        Success = false,
                        Messaggio = "Credenziali non valide."
                    });
                }

                // Se il cliente non ha una password impostata (vecchio cliente o registrato in negozio)
                if (string.IsNullOrEmpty(cliente.PasswordHash))
                {
                    return Unauthorized(new LoginClienteResponse
                    {
                        Success = false,
                        Messaggio = "Account non ancora attivato online. Procedi con la registrazione."
                    });
                }

                // Verifica password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, cliente.PasswordHash))
                {
                    return Unauthorized(new LoginClienteResponse
                    {
                        Success = false,
                        Messaggio = "Credenziali non valide."
                    });
                }

                var token = GeneraJwtToken(cliente);

                return Ok(new LoginClienteResponse
                {
                    Success = true,
                    Token = token,
                    ClienteId = cliente.Id,
                    Nome = cliente.Nome,
                    Cognome = cliente.Cognome,
                    CodiceFidelity = cliente.CodiceFidelity,
                    PuntiTotali = cliente.PuntiTotali,
                    Messaggio = "Benvenuto!"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new LoginClienteResponse
                {
                    Success = false,
                    Messaggio = "Errore durante il login."
                });
            }
        }

        /// <summary>
        /// Registrazione self-service per clienti
        /// </summary>
        [HttpPost("register/cliente")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginClienteResponse>> RegisterCliente([FromBody] RegisterClienteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (request.HasExistingCard)
                {
                    // === ATTIVAZIONE CARD ESISTENTE ===
                    if (string.IsNullOrWhiteSpace(request.ExistingFidelityCode))
                        return BadRequest(new { success = false, messaggio = "Codice Fidelity obbligatorio." });

                    var cliente = await _context.Clienti
                        .FirstOrDefaultAsync(c => c.CodiceFidelity == request.ExistingFidelityCode.Trim() && c.Attivo);

                    if (cliente == null)
                        return BadRequest(new { success = false, messaggio = "Codice Fidelity non trovato." });

                    // Verifica corrispondenza Email
                    if (!cliente.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                         return BadRequest(new { success = false, messaggio = "L'email inserita non corrisponde a quella associata alla card." });

                    // Verifica se già attivo online
                    if (!string.IsNullOrEmpty(cliente.PasswordHash))
                        return BadRequest(new { success = false, messaggio = "Utente già registrato. Effettua il login." });

                    // Imposta password
                    cliente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    cliente.PrivacyAccettata = request.PrivacyAccepted; // Aggiorna consenso privacy
                    
                    await _context.SaveChangesAsync();
                     
                     // Login automatico
                     var token = GeneraJwtToken(cliente);
                     return Ok(new LoginClienteResponse
                     {
                         Success = true,
                         Token = token,
                         ClienteId = cliente.Id,
                         Nome = cliente.Nome,
                         Cognome = cliente.Cognome,
                         CodiceFidelity = cliente.CodiceFidelity,
                         PuntiTotali = cliente.PuntiTotali,
                         Messaggio = "Account attivato con successo!"
                     });
                }
                else
                {
                    // === NUOVA REGISTRAZIONE ===
                    var emailEsistente = await _context.Clienti.AnyAsync(c => c.Email == request.Email);
                    if (emailEsistente)
                        return BadRequest(new { success = false, messaggio = "Email già registrata." });

                    var nuovoCodice = await GeneraCodiceFidelityUnivocoAsync();

                    var nuovoCliente = new Cliente
                    {
                        Nome = request.Nome,
                        Cognome = request.Cognome,
                        Email = request.Email,
                        Telefono = request.Telefono ?? "",
                        CodiceFidelity = nuovoCodice,
                        DataRegistrazione = DateTime.UtcNow,
                        PuntiTotali = 0,
                        Attivo = true,
                        PrivacyAccettata = request.PrivacyAccepted,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                        PuntoVenditaRegistrazioneId = request.PuntoVenditaId, // Nullable se online globale
                        ResponsabileRegistrazioneId = null // Registrato online
                    };
                    
                    // Assegna punto vendita di default se non specificato (es: primo PV) o lascia null se gestito
                    // Per ora lasciamo null se modello lo permette, altrimenti cerchiamo un default
                    if (nuovoCliente.PuntoVenditaRegistrazioneId == null)
                    {
                        // Fallback: Assegna al primo PV ("Sede Centrale" o simile) se vogliamo forzare
                        var defaultPv = await _context.PuntiVendita.FirstOrDefaultAsync();
                        if (defaultPv != null) nuovoCliente.PuntoVenditaRegistrazioneId = defaultPv.Id;
                    }

                    _context.Clienti.Add(nuovoCliente);
                    await _context.SaveChangesAsync();

                    try 
                    {
                        // Invia Welcome Email + Card (opzionale se fallisce non bloccare tutto)
                        // Per generare card serve caricare PuntoVendita
                        var pv = await _context.PuntiVendita.FindAsync(nuovoCliente.PuntoVenditaRegistrazioneId);
                        
                        var cardDigitale = await _cardGenerator.GeneraCardDigitaleAsync(nuovoCliente, pv);
                        await _emailService.InviaEmailBenvenutoAsync(nuovoCliente.Email, nuovoCliente.Nome, nuovoCliente.CodiceFidelity, cardDigitale);
                    }
                    catch {} // Ignora errori email per non bloccare registrazione

                    // Login automatico
                    var token = GeneraJwtToken(nuovoCliente);
                    return Ok(new LoginClienteResponse
                    {
                        Success = true,
                        Token = token,
                        ClienteId = nuovoCliente.Id,
                        Nome = nuovoCliente.Nome,
                        Cognome = nuovoCliente.Cognome,
                        CodiceFidelity = nuovoCliente.CodiceFidelity,
                        PuntiTotali = 0,
                        Messaggio = "Benvenuto in Fidelis!"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, messaggio = "Errore durante la registrazione.", errore = ex.Message });
            }
        }
        
        private string GeneraJwtToken(Cliente cliente)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
                new Claim(ClaimTypes.Name, cliente.Email),
                new Claim(ClaimTypes.Role, "Cliente"),
                new Claim("CodiceFidelity", cliente.CodiceFidelity),
                new Claim("NomeCompleto", $"{cliente.Nome} {cliente.Cognome}")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30), // Clienti rimangono loggati più a lungo
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GeneraCodiceFidelityUnivocoAsync()
        {
            string codice;
            bool esiste;
            do
            {
                var numero = new Random().Next(100000000, 999999999);
                codice = $"SUN{numero}";
                esiste = await _context.Clienti.AnyAsync(c => c.CodiceFidelity == codice);
            }
            while (esiste);
            return codice;
        }
    }
}