// Fidelity.Server/Controllers/ClientiController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fidelity.Server.Data;
using Fidelity.Shared.DTOs;
using System.Security.Claims;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Cerca un cliente per codice fedeltà o email
        /// </summary>
        [HttpGet("cerca")]
        public async Task<ActionResult<ClienteResponse>> CercaCliente([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                return BadRequest(new { messaggio = "Query di ricerca troppo breve. Minimo 3 caratteri." });
            }

            try
            {
                // Ottieni PuntoVenditaId del responsabile autenticato
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;

                // Cerca per codice fedeltà o email
                var cliente = await _context.Clienti
                    .Include(c => c.PuntoVenditaRegistrazione)
                    .Where(c => 
                        (c.CodiceFidelity == query || c.Email.Contains(query)) &&
                        (ruolo == "Admin" || c.PuntoVenditaRegistrazioneId.ToString() == puntoVenditaIdClaim))
                    .Select(c => new ClienteResponse
                    {
                        Id = c.Id,
                        CodiceFidelity = c.CodiceFidelity,
                        NomeCompleto = $"{c.Nome} {c.Cognome}",
                        Email = c.Email,
                        Telefono = c.Telefono,
                        PuntiTotali = c.PuntiTotali,
                        DataRegistrazione = c.DataRegistrazione,
                        PuntoVenditaRegistrazione = c.PuntoVenditaRegistrazione.Nome,
                        PuntoVenditaCodice = c.PuntoVenditaRegistrazione.Codice,
                        Attivo = c.Attivo
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound(new { messaggio = "Cliente non trovato." });
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante la ricerca del cliente.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Ottieni tutti i clienti del proprio punto vendita
        /// </summary>
        [HttpGet("mio-punto-vendita")]
        public async Task<ActionResult<List<ClienteResponse>>> GetClientiMioPuntoVendita()
        {
            try
            {
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;

                if (ruolo != "Admin" && string.IsNullOrEmpty(puntoVenditaIdClaim))
                {
                    return BadRequest(new { messaggio = "Punto vendita non trovato." });
                }

                IQueryable<Shared.Models.Cliente> query = _context.Clienti
                    .Include(c => c.PuntoVenditaRegistrazione);

                // Se non è Admin, filtra per punto vendita
                if (ruolo != "Admin")
                {
                    var puntoVenditaId = int.Parse(puntoVenditaIdClaim!);
                    query = query.Where(c => c.PuntoVenditaRegistrazioneId == puntoVenditaId);
                }

                var clienti = await query
                    .Where(c => c.Attivo)
                    .OrderByDescending(c => c.DataRegistrazione)
                    .Select(c => new ClienteResponse
                    {
                        Id = c.Id,
                        CodiceFidelity = c.CodiceFidelity,
                        NomeCompleto = $"{c.Nome} {c.Cognome}",
                        Email = c.Email,
                        Telefono = c.Telefono,
                        PuntiTotali = c.PuntiTotali,
                        DataRegistrazione = c.DataRegistrazione,
                        PuntoVenditaRegistrazione = c.PuntoVenditaRegistrazione.Nome,
                        PuntoVenditaCodice = c.PuntoVenditaRegistrazione.Codice,
                        Attivo = c.Attivo
                    })
                    .ToListAsync();

                return Ok(clienti);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il caricamento dei clienti.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Ottieni dettagli di un cliente specifico
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteResponse>> GetCliente(int id)
        {
            try
            {
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;

                var cliente = await _context.Clienti
                    .Include(c => c.PuntoVenditaRegistrazione)
                    .Where(c => c.Id == id &&
                        (ruolo == "Admin" || c.PuntoVenditaRegistrazioneId.ToString() == puntoVenditaIdClaim))
                    .Select(c => new ClienteResponse
                    {
                        Id = c.Id,
                        CodiceFidelity = c.CodiceFidelity,
                        NomeCompleto = $"{c.Nome} {c.Cognome}",
                        Email = c.Email,
                        Telefono = c.Telefono,
                        PuntiTotali = c.PuntiTotali,
                        DataRegistrazione = c.DataRegistrazione,
                        PuntoVenditaRegistrazione = c.PuntoVenditaRegistrazione.Nome,
                        PuntoVenditaCodice = c.PuntoVenditaRegistrazione.Codice,
                        Attivo = c.Attivo
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound(new { messaggio = "Cliente non trovato." });
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il caricamento del cliente.", errore = ex.Message });
            }
        }
    }
}
