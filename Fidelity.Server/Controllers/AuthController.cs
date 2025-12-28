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

                // Check if account is locked
                if (responsabile.AccountLockedUntil.HasValue && responsabile.AccountLockedUntil.Value > DateTime.UtcNow)
                {
                    var remainingMinutes = (int)(responsabile.AccountLockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Messaggio = $"Account temporaneamente bloccato. Riprova tra {remainingMinutes} minuti."
                    });
                }

                // Verifica password con BCrypt
                if (!BCrypt.Net.BCrypt.Verify(request.Password, responsabile.PasswordHash))
                {
                    // Increment failed attempts
                    responsabile.FailedLoginAttempts++;
                    
                    // Lock account after 5 failed attempts
                    if (responsabile.FailedLoginAttempts >= 5)
                    {
                        responsabile.AccountLockedUntil = DateTime.UtcNow.AddMinutes(15);
                        await _context.SaveChangesAsync();
                        
                        return Unauthorized(new LoginResponse
                        {
                            Success = false,
                            Messaggio = "Troppi tentativi falliti. Account bloccato per 15 minuti."
                        });
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Messaggio = $"Username o password non corretti. Tentativi rimasti: {5 - responsabile.FailedLoginAttempts}"
                    });
                }

                // Reset failed attempts on successful login
                responsabile.FailedLoginAttempts = 0;
                responsabile.AccountLockedUntil = null;

                // Aggiorna ultimo accesso
                responsabile.UltimoAccesso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Ottieni il primo punto vendita associato (se esiste)
                var primoPuntoVendita = responsabile.ResponsabilePuntiVendita?.FirstOrDefault()?.PuntoVendita;

                // Genera JWT Token e Refresh Token
                var (token, jwtId) = GeneraJwtToken(responsabile, primoPuntoVendita?.Id);
                var refreshToken = await GeneraRefreshTokenAsync(jwtId, responsabileId: responsabile.Id);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
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
        /// Cambia password per cliente autenticato
        /// </summary>
        [HttpPost("cliente/cambia-password")]
        [Authorize(Roles = "Cliente")] // Assicurati che solo i clienti possano usare questo endpoint
        public async Task<IActionResult> CambiaPasswordCliente([FromBody] CambiaPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var cliente = await _context.Clienti.FindAsync(clienteId);

                if (cliente == null)
                    return NotFound(new { success = false, messaggio = "Cliente non trovato." });

                // Verifica password attuale
                if (!BCrypt.Net.BCrypt.Verify(request.PasswordAttuale, cliente.PasswordHash))
                {
                    return BadRequest(new { success = false, messaggio = "Password attuale non corretta." });
                }

                // Salva nuova password
                cliente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuovaPassword);
                await _context.SaveChangesAsync();

                // Rigenera token
                var (nuovoToken, jwtId) = GeneraJwtToken(cliente);
                var refreshToken = await GeneraRefreshTokenAsync(jwtId, clienteId: cliente.Id);

                return Ok(new
                {
                    success = true,
                    messaggio = "Password aggiornata con successo.",
                    token = nuovoToken,
                    refreshToken
                });
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

                // Check if account is locked
                if (cliente.AccountLockedUntil.HasValue && cliente.AccountLockedUntil.Value > DateTime.UtcNow)
                {
                    var remainingMinutes = (int)(cliente.AccountLockedUntil.Value - DateTime.UtcNow).TotalMinutes;
                    return Unauthorized(new LoginClienteResponse
                    {
                        Success = false,
                        Messaggio = $"Account temporaneamente bloccato. Riprova tra {remainingMinutes} minuti."
                    });
                }

                // Verifica password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, cliente.PasswordHash))
                {
                    // Increment failed attempts
                    cliente.FailedLoginAttempts++;
                    
                    // Lock account after 5 failed attempts
                    if (cliente.FailedLoginAttempts >= 5)
                    {
                        cliente.AccountLockedUntil = DateTime.UtcNow.AddMinutes(15);
                        await _context.SaveChangesAsync();
                        
                        return Unauthorized(new LoginClienteResponse
                        {
                            Success = false,
                            Messaggio = "Troppi tentativi falliti. Account bloccato per 15 minuti."
                        });
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    return Unauthorized(new LoginClienteResponse
                    {
                        Success = false,
                        Messaggio = $"Password non corretta. Tentativi rimasti: {5 - cliente.FailedLoginAttempts}"
                    });
                }

                // Reset failed attempts on successful login
                cliente.FailedLoginAttempts = 0;
                cliente.AccountLockedUntil = null;

                // Genera JWT Token
                var (token, jwtId) = GeneraJwtToken(cliente);
                var refreshToken = await GeneraRefreshTokenAsync(jwtId, clienteId: cliente.Id);

                return Ok(new LoginClienteResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ClienteId = cliente.Id,
                    CodiceFidelity = cliente.CodiceFidelity,
                    Email = cliente.Email,
                    Nome = cliente.Nome,
                    Cognome = cliente.Cognome,
                    PuntiTotali = cliente.PuntiTotali,
                    Messaggio = "Login effettuato con successo."
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
                     var (token, jwtId) = GeneraJwtToken(cliente);
                     var refreshToken = await GeneraRefreshTokenAsync(jwtId, clienteId: cliente.Id);
                     
                     return Ok(new LoginClienteResponse
                     {
                         Success = true,
                         Token = token,
                         RefreshToken = refreshToken,
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
                    var (token, jwtId) = GeneraJwtToken(nuovoCliente);
                    var refreshToken = await GeneraRefreshTokenAsync(jwtId, clienteId: nuovoCliente.Id);
                    
                    return Ok(new LoginClienteResponse
                    {
                        Success = true,
                        Token = token,
                        RefreshToken = refreshToken,
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
        
        // ===== REFRESH TOKEN METHODS =====
        
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest(new { messaggio = "Refresh token mancante" });

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.Cliente)
                .Include(rt => rt.Responsabile)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null)
                return Unauthorized(new { messaggio = "Refresh token non valido" });

            if (storedToken.IsUsed)
                return Unauthorized(new { messaggio = "Refresh token già utilizzato" });

            if (storedToken.IsRevoked)
                return Unauthorized(new { messaggio = "Refresh token revocato" });

            if (storedToken.ExpiryDate < DateTime.UtcNow)
                return Unauthorized(new { messaggio = "Refresh token scaduto" });

            // Mark token as used
            storedToken.IsUsed = true;
            await _context.SaveChangesAsync();

            // Generate new tokens
            if (storedToken.ClienteId.HasValue && storedToken.Cliente != null)
            {
                var (newToken, jwtId) = GeneraJwtToken(storedToken.Cliente);
                var newRefreshToken = await GeneraRefreshTokenAsync(jwtId, clienteId: storedToken.ClienteId.Value);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    Messaggio = "Token rinnovato con successo"
                });
            }
            else if (storedToken.ResponsabileId.HasValue && storedToken.Responsabile != null)
            {
                var responsabile = await _context.Responsabili
                    .Include(r => r.ResponsabilePuntiVendita)
                        .ThenInclude(rp => rp.PuntoVendita)
                    .FirstOrDefaultAsync(r => r.Id == storedToken.ResponsabileId.Value);

                if (responsabile == null)
                    return Unauthorized(new { messaggio = "Utente non trovato" });

                var primoPuntoVendita = responsabile.ResponsabilePuntiVendita?.FirstOrDefault()?.PuntoVendita;
                var (newToken, jwtId) = GeneraJwtToken(responsabile, primoPuntoVendita?.Id);
                var newRefreshToken = await GeneraRefreshTokenAsync(jwtId, responsabileId: storedToken.ResponsabileId.Value);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ResponsabileId = responsabile.Id,
                    Username = responsabile.Username,
                    Ruolo = responsabile.Ruolo,
                    Messaggio = "Token rinnovato con successo"
                });
            }

            return Unauthorized(new { messaggio = "Refresh token non valido" });
        }

        private async Task<string> GeneraRefreshTokenAsync(string jwtId, int? clienteId = null, int? responsabileId = null)
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "-" + Guid.NewGuid().ToString(),
                JwtId = jwtId,
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsUsed = false,
                IsRevoked = false,
                ClienteId = clienteId,
                ResponsabileId = responsabileId
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }

        private (string token, string jwtId) GeneraJwtToken(Responsabile responsabile, int? puntoVenditaId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var jwtId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(ClaimTypes.NameIdentifier, responsabile.Id.ToString()),
                new Claim(ClaimTypes.Name, responsabile.Username),
                new Claim(ClaimTypes.Role, responsabile.Ruolo),
                new Claim("NomeCompleto", responsabile.NomeCompleto ?? "")
            };

            if (puntoVenditaId.HasValue)
            {
                claims.Add(new Claim("PuntoVenditaId", puntoVenditaId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), jwtId);
        }

        private (string token, string jwtId) GeneraJwtToken(Cliente cliente)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var jwtId = Guid.NewGuid().ToString();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
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
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), jwtId);
        }
    }
}

// DTO for Refresh Token Request
public record RefreshTokenRequest(string RefreshToken);