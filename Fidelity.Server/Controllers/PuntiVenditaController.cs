// Fidelity.Server/Controllers/PuntiVenditaController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Shared.DTOs;
using Fidelity.Domain.Entities;
using System.Security.Claims;
using BCrypt.Net;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PuntiVenditaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PuntiVenditaController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Ottieni tutti i punti vendita con statistiche clienti
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PuntoVenditaResponse>>> GetAll()
        {
            try
            {
                var puntiVendita = await _context.PuntiVendita
                    .OrderBy(pv => pv.Codice)
                    .ProjectTo<PuntoVenditaResponse>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(puntiVendita);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il caricamento dei punti vendita.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Ottieni lista pubblica punti vendita (per registrazione)
        /// </summary>
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<List<object>>> GetPublicList()
        {
            try
            {
                var puntiVendita = await _context.PuntiVendita
                    .Where(p => p.Attivo)
                    .OrderBy(p => p.Nome)
                    .Select(p => new 
                    {
                        p.Id,
                        p.Nome,
                        p.Citta
                    })
                    .ToListAsync();

                return Ok(puntiVendita);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il caricamento dei punti vendita.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Ottieni dettagli di un punto vendita specifico
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PuntoVenditaResponse>> GetById(int id)
        {
            try
            {
                var puntoVendita = await _context.PuntiVendita
                    .Where(pv => pv.Id == id)
                    .ProjectTo<PuntoVenditaResponse>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                if (puntoVendita == null)
                    return NotFound(new { messaggio = "Punto vendita non trovato." });

                return Ok(puntoVendita);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il caricamento del punto vendita.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuovo punto vendita
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PuntoVenditaResponse>> Create([FromBody] PuntoVenditaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verifica che il codice non esista già
                if (await _context.PuntiVendita.AnyAsync(pv => pv.Codice == request.Codice))
                {
                    return BadRequest(new { messaggio = $"Esiste già un punto vendita con codice '{request.Codice}'." });
                }

                // Genera username per il responsabile (es: NE03 -> RE03)
                var responsabileUsername = "RE" + request.Codice.Substring(2);

                // Verifica che il responsabile con questo username non esista già
                if (await _context.Responsabili.AnyAsync(r => r.Username == responsabileUsername))
                {
                    return BadRequest(new { messaggio = $"Esiste già un responsabile con username '{responsabileUsername}'. Impossibile creare il punto vendita." });
                }

                var puntoVendita = new PuntoVendita
                {
                    Codice = request.Codice,
                    Nome = request.Nome,
                    Citta = request.Citta,
                    Indirizzo = request.Indirizzo,
                    Telefono = request.Telefono,
                    Attivo = request.Attivo,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PuntiVendita.Add(puntoVendita);
                await _context.SaveChangesAsync();

                // Crea automaticamente il responsabile associato
                try
                {
                    var responsabile = new Responsabile
                    {
                        Username = responsabileUsername,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Suns2024!"),
                        NomeCompleto = "", // Empty string instead of null (will be filled during profile completion)
                        Email = $"responsabile.{responsabileUsername.ToLower()}@sunscompany.com",
                        Ruolo = "Responsabile",
                        Attivo = true,
                        RichiestaResetPassword = true // Forza cambio password al primo accesso
                    };

                    Console.WriteLine($"[DEBUG] Creating responsabile: {responsabile.Username}, Email: {responsabile.Email}");
                    
                    _context.Responsabili.Add(responsabile);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[DEBUG] Responsabile created with ID: {responsabile.Id}");

                    // Crea l'associazione nella junction table
                    var link = new ResponsabilePuntoVendita
                    {
                        ResponsabileId = responsabile.Id,
                        PuntoVenditaId = puntoVendita.Id,
                        DataAssociazione = DateTime.UtcNow,
                        Principale = true
                    };

                    _context.ResponsabilePuntiVendita.Add(link);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"[PuntiVenditaController] Punto vendita '{puntoVendita.Codice}' creato con responsabile '{responsabile.Username}'");
                }
                catch (Exception exResp)
                {
                    Console.WriteLine($"[ERROR] Failed to create responsabile: {exResp.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {exResp.StackTrace}");
                    if (exResp.InnerException != null)
                    {
                        Console.WriteLine($"[ERROR] Inner exception: {exResp.InnerException.Message}");
                    }
                    
                    // Rollback - remove punto vendita
                    _context.PuntiVendita.Remove(puntoVendita);
                    await _context.SaveChangesAsync();
                    
                    return StatusCode(500, new { messaggio = "Errore durante la creazione del responsabile.", errore = exResp.Message });
                }

                var response = new PuntoVenditaResponse
                {
                    Id = puntoVendita.Id,
                    Codice = puntoVendita.Codice,
                    Nome = puntoVendita.Nome,
                    Citta = puntoVendita.Citta,
                    Indirizzo = puntoVendita.Indirizzo,
                    Telefono = puntoVendita.Telefono,
                    Attivo = puntoVendita.Attivo,
                    NumeroClienti = 0,
                    DataCreazione = puntoVendita.CreatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = puntoVendita.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante la creazione del punto vendita.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Aggiorna un punto vendita esistente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<PuntoVenditaResponse>> Update(int id, [FromBody] PuntoVenditaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var puntoVendita = await _context.PuntiVendita.FindAsync(id);
                if (puntoVendita == null)
                    return NotFound(new { messaggio = "Punto vendita non trovato." });

                // Verifica che il nuovo codice non esista già (escludendo il punto vendita corrente)
                if (await _context.PuntiVendita.AnyAsync(pv => pv.Codice == request.Codice && pv.Id != id))
                {
                    return BadRequest(new { messaggio = $"Esiste già un punto vendita con codice '{request.Codice}'." });
                }

                puntoVendita.Codice = request.Codice;
                puntoVendita.Nome = request.Nome;
                puntoVendita.Citta = request.Citta;
                puntoVendita.Indirizzo = request.Indirizzo;
                puntoVendita.Telefono = request.Telefono;
                puntoVendita.Attivo = request.Attivo;

                await _context.SaveChangesAsync();

                var numeroClienti = await _context.Clienti.CountAsync(c => c.PuntoVenditaRegistrazioneId == id && c.Attivo);

                var response = new PuntoVenditaResponse
                {
                    Id = puntoVendita.Id,
                    Codice = puntoVendita.Codice,
                    Nome = puntoVendita.Nome,
                    Citta = puntoVendita.Citta,
                    Indirizzo = puntoVendita.Indirizzo,
                    Telefono = puntoVendita.Telefono,
                    Attivo = puntoVendita.Attivo,
                    NumeroClienti = numeroClienti,
                    DataCreazione = puntoVendita.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante l'aggiornamento del punto vendita.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un punto vendita
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var puntoVendita = await _context.PuntiVendita
                    .Include(pv => pv.ClientiRegistrati)
                    .FirstOrDefaultAsync(pv => pv.Id == id);

                if (puntoVendita == null)
                    return NotFound(new { messaggio = "Punto vendita non trovato." });

                // Verifica se ci sono clienti associati
                if (puntoVendita.ClientiRegistrati.Any())
                {
                    return BadRequest(new { 
                        messaggio = $"Impossibile eliminare il punto vendita. Ci sono {puntoVendita.ClientiRegistrati.Count} clienti associati.",
                        numeroClienti = puntoVendita.ClientiRegistrati.Count
                    });
                }

                // Rimuovi associazioni con responsabili (junction table)
                var associazioni = await _context.ResponsabilePuntiVendita
                    .Where(rp => rp.PuntoVenditaId == id)
                    .ToListAsync();

                if (associazioni.Any())
                {
                    _context.ResponsabilePuntiVendita.RemoveRange(associazioni);
                }

                _context.PuntiVendita.Remove(puntoVendita);
                await _context.SaveChangesAsync();

                return Ok(new { messaggio = "Punto vendita eliminato con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante l'eliminazione del punto vendita.", errore = ex.Message });
            }
        }
    }
}
