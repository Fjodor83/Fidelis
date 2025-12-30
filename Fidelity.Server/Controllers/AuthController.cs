using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Shared.DTOs;
using Fidelity.Domain.Entities;
using BCrypt.Net;
using Fidelity.Server.Services;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Fidelity.Application.Clienti.Commands.RegistraCliente;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ISender _sender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ISender sender,
            ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _sender = sender;
            _logger = logger;
        }


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
                if (responsabile.IsAccountLocked())
                {
                    var remainingMinutes = (int)(responsabile.AccountLockedUntil!.Value - DateTime.UtcNow).TotalMinutes;
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Messaggio = $"Account temporaneamente bloccato. Riprova tra {remainingMinutes} minuti."
                    });
                }

                // Verifica password con BCrypt
                if (!BCrypt.Net.BCrypt.Verify(request.Password, responsabile.PasswordHash))
                {
                    responsabile.IncrementaLoginFallito();
                    await _context.SaveChangesAsync();
                    
                    if (responsabile.IsAccountLocked())
                    {
                        return Unauthorized(new LoginResponse
                        {
                            Success = false,
                            Messaggio = "Troppi tentativi falliti. Account bloccato per 15 minuti."
                        });
                    }
                    
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Messaggio = $"Username o password non corretti. Tentativi rimasti: {5 - responsabile.FailedLoginAttempts}"
                    });
                }

                // Reset failed attempts on successful login
                responsabile.RegistraAccesso(HttpContext.Connection.RemoteIpAddress?.ToString());
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
                    NomeCompleto = responsabile.NomeCompleto ?? string.Empty,
                    Ruolo = responsabile.Ruolo,
                    PuntoVenditaId = primoPuntoVendita?.Id,
                    PuntoVenditaCodice = primoPuntoVendita?.Codice ?? string.Empty,
                    PuntoVenditaNome = primoPuntoVendita?.Nome ?? string.Empty,
                    RichiestaResetPassword = responsabile.RichiestaResetPassword,
                    ProfiloIncompleto = string.IsNullOrWhiteSpace(responsabile.NomeCompleto),
                    Messaggio = "Login effettuato con successo."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il login per {Username}", request.Username);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Messaggio = "Errore durante il login."
                });
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
                _logger.LogError(ex, "Errore durante il cambio password cliente");
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

                // Valida कि il profilo sia incompleto (NomeCompleto vuoto)
                if (!string.IsNullOrWhiteSpace(responsabile.NomeCompleto))
                    return BadRequest(new { success = false, messaggio = "Profilo già completato." });

                // Aggiorna con nome reale
                responsabile.NomeCompleto = $"{request.Nome} {request.Cognome}";
                await _context.SaveChangesAsync();

                return Ok(new { success = true, messaggio = "Profilo completato con successo.", nomeCompleto = responsabile.NomeCompleto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il completamento del profilo");
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
                if (cliente.IsAccountLocked())
                {
                    var remainingMinutes = (int)(cliente.AccountLockedUntil!.Value - DateTime.UtcNow).TotalMinutes;
                    return Unauthorized(new LoginClienteResponse
                    {
                        Success = false,
                        Messaggio = $"Account temporaneamente bloccato. Riprova tra {remainingMinutes} minuti."
                    });
                }

                // Verifica password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, cliente.PasswordHash))
                {
                    cliente.IncrementaLoginFallito();
                    await _context.SaveChangesAsync();
                    
                    if (cliente.IsAccountLocked())
                    {
                        return Unauthorized(new LoginClienteResponse
                        {
                            Success = false,
                            Messaggio = "Troppi tentativi falliti. Account bloccato per 15 minuti."
                        });
                    }
                    
                    return Unauthorized(new LoginClienteResponse
                    {
                        Success = false,
                        Messaggio = $"Password non corretta. Tentativi rimasti: {5 - cliente.FailedLoginAttempts}"
                    });
                }

                // Reset failed attempts on successful login
                cliente.SbloccaAccount();
                await _context.SaveChangesAsync();

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
                var command = new RegistraClienteCommand
                {
                    Nome = request.Nome,
                    Cognome = request.Cognome,
                    Email = request.Email,
                    Password = request.Password,
                    Telefono = request.Telefono,
                    PrivacyAccepted = request.PrivacyAccepted,
                    PuntoVenditaId = request.PuntoVenditaId,
                    HasExistingCard = request.HasExistingCard,
                    ExistingFidelityCode = request.ExistingFidelityCode
                };

                var result = await _sender.Send(command);

                if (!result.Succeeded || result.Data == null)
                {
                    return BadRequest(new { success = false, messaggio = string.Join(", ", result.Errors) });
                }

                // Recupera il cliente dal database per generare il token
                var cliente = await _context.Clienti.FindAsync(result.Data.ClienteId);
                
                if (cliente == null)
                {
                    return StatusCode(500, new { success = false, messaggio = "Errore durante il recupero del cliente dopo la registrazione." });
                }

                // Genera JWT Token
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
                    Messaggio = request.HasExistingCard ? "Account attivato con successo!" : "Benvenuto in Fidelis!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, messaggio = "Errore durante la registrazione.", errore = ex.Message });
            }
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
            var securityKey = GetSecurityKey();
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            
            var jwtId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(ClaimTypes.NameIdentifier, responsabile.Id.ToString()),
                new Claim(ClaimTypes.Name, responsabile.Username),
                new Claim(ClaimTypes.Role, responsabile.Ruolo),
                new Claim("NomeCompleto", responsabile.NomeCompleto ?? ""),
                new Claim("richiestaResetPassword", responsabile.RichiestaResetPassword ? "true" : "false"),
                new Claim("profiloIncompleto", string.IsNullOrWhiteSpace(responsabile.NomeCompleto) ? "true" : "false")
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
            var securityKey = GetSecurityKey();
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "S6781:JWT secret keys should not be disclosed", Justification = "Key is retrieved from IConfiguration abstraction, which supports secure sources like Environment Variables and Key Vault.")]
        private SymmetricSecurityKey GetSecurityKey()
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key) || key.Length < 32)
            {
                throw new InvalidOperationException("JWT Key must be configured and at least 32 characters long.");
            }
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }
    }

    public record RefreshTokenRequest(string RefreshToken);
}