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
using Microsoft.AspNetCore.Authorization;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
    }
}