// Fidelity.Server/Controllers/ResponsabiliController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Shared.DTOs;
using Fidelity.Domain.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Allow authenticated users (Admin + Responsabile)
    public class ResponsabiliController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ResponsabiliController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Ottieni tutti i responsabili con i loro punti vendita
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ResponsabileDetailResponse>>> GetAll()
        {
            // ... (keep implementation)
            try
            {
                var responsabili = await _context.Responsabili
                    .Where(r => r.Ruolo == "Responsabile")
                    .OrderBy(r => r.Username)
                    .ProjectTo<ResponsabileDetailResponse>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return Ok(responsabili);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il recupero dei responsabili.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuovo responsabile
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponsabileDetailResponse>> Create([FromBody] ResponsabileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Verifica username esistente (inclusi eliminati)
                var existingUser = await _context.Responsabili
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Username == request.Username);

                if (existingUser != null)
                {
                    if (!existingUser.IsDeleted)
                    {
                         return BadRequest(new { messaggio = $"Username '{request.Username}' già esistente." });
                    }
                    
                    // Restore logic if deleted
                    // Verify email constraint first (if email is different from old one)
                     var existingEmailCheck = await _context.Responsabili
                        .IgnoreQueryFilters()
                        .AnyAsync(r => r.Email == request.Email && r.Id != existingUser.Id); // Check other users

                    if (existingEmailCheck)
                        return BadRequest(new { messaggio = $"Email '{request.Email}' già utilizzata da un altro utente." });

                    existingUser.Restore();
                    existingUser.NomeCompleto = request.NomeCompleto;
                    existingUser.Email = request.Email;
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password ?? "Suns2024!");
                    existingUser.Attivo = request.Attivo;
                    existingUser.RichiestaResetPassword = request.RichiestaResetPassword;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    // Update stores
                    // Remove old links
                     var oldLinks = await _context.ResponsabilePuntiVendita.Where(rp => rp.ResponsabileId == existingUser.Id).ToListAsync();
                    _context.ResponsabilePuntiVendita.RemoveRange(oldLinks);
                    
                    // Add new links
                    foreach (var pvId in request.PuntiVenditaIds)
                    {
                        _context.ResponsabilePuntiVendita.Add(new ResponsabilePuntoVendita
                        {
                            ResponsabileId = existingUser.Id,
                            PuntoVenditaId = pvId,
                            DataAssociazione = DateTime.UtcNow,
                            Principale = request.PuntiVenditaIds.IndexOf(pvId) == 0
                        });
                    }

                    await _context.SaveChangesAsync();
                     var restored = await _context.Responsabili
                        .Include(r => r.ResponsabilePuntiVendita)
                            .ThenInclude(rp => rp.PuntoVendita)
                        .FirstAsync(r => r.Id == existingUser.Id);

                    var responseRestored = _mapper.Map<ResponsabileDetailResponse>(restored);
                    return CreatedAtAction(nameof(GetAll), new { id = responseRestored.Id }, responseRestored);
                }

                // Verifica email univoca
                var existingEmail = await _context.Responsabili
                    .IgnoreQueryFilters()
                    .AnyAsync(r => r.Email == request.Email);

                if (existingEmail)
                    return BadRequest(new { messaggio = $"Email '{request.Email}' già esistente." });

                // Verifica che tutti i punti vendita esistano
                var puntiVenditaEsistenti = await _context.PuntiVendita
                    .Where(pv => request.PuntiVenditaIds.Contains(pv.Id))
                    .CountAsync();

                if (puntiVenditaEsistenti != request.PuntiVenditaIds.Count)
                    return BadRequest(new { messaggio = "Uno o più punti vendita selezionati non esistono." });

                // Crea responsabile
                var responsabile = new Responsabile
                {
                    Username = request.Username,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password ?? "Suns2024!"),
                    NomeCompleto = request.NomeCompleto,
                    Email = request.Email,
                    Ruolo = "Responsabile",
                    Attivo = request.Attivo,
                    RichiestaResetPassword = request.RichiestaResetPassword
                };

                _context.Responsabili.Add(responsabile);
                await _context.SaveChangesAsync();

                // Crea associazioni con i punti vendita
                foreach (var pvId in request.PuntiVenditaIds)
                {
                    var link = new ResponsabilePuntoVendita
                    {
                        ResponsabileId = responsabile.Id,
                        PuntoVenditaId = pvId,
                        DataAssociazione = DateTime.UtcNow,
                        Principale = request.PuntiVenditaIds.IndexOf(pvId) == 0 // Assume primo come principale
                    };
                    _context.ResponsabilePuntiVendita.Add(link);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"[ResponsabiliController] Responsabile '{responsabile.Username}' creato e assegnato a {request.PuntiVenditaIds.Count} punti vendita");

                // Ricarica il responsabile con le relazioni per la risposta
                var created = await _context.Responsabili
                    .Include(r => r.ResponsabilePuntiVendita)
                        .ThenInclude(rp => rp.PuntoVendita)
                    .FirstAsync(r => r.Id == responsabile.Id);

                var response = _mapper.Map<ResponsabileDetailResponse>(created);

                return CreatedAtAction(nameof(GetAll), new { id = response.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante la creazione del responsabile.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Aggiorna un responsabile esistente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponsabileDetailResponse>> Update(int id, [FromBody] ResponsabileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var responsabile = await _context.Responsabili
                    .Include(r => r.ResponsabilePuntiVendita)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (responsabile == null)
                    return NotFound(new { messaggio = "Responsabile non trovato." });

                // Non permettere modifica admin
                if (responsabile.Ruolo == "Admin")
                    return BadRequest(new { messaggio = "Non è possibile modificare un amministratore." });

                // Verifica email univoca (escluso il corrente)
                var existingEmail = await _context.Responsabili
                    .IgnoreQueryFilters()
                    .AnyAsync(r => r.Email == request.Email && r.Id != id);

                if (existingEmail)
                    return BadRequest(new { messaggio = $"Email '{request.Email}' già esistente." });

                // Verifica che tutti i punti vendita esistano
                var puntiVenditaEsistenti = await _context.PuntiVendita
                    .Where(pv => request.PuntiVenditaIds.Contains(pv.Id))
                    .CountAsync();

                if (puntiVenditaEsistenti != request.PuntiVenditaIds.Count)
                    return BadRequest(new { messaggio = "Uno o più punti vendita selezionati non esistono." });

                // Aggiorna dati responsabile
                responsabile.NomeCompleto = request.NomeCompleto;
                responsabile.Email = request.Email;
                responsabile.Attivo = request.Attivo;
                responsabile.RichiestaResetPassword = request.RichiestaResetPassword;

                // Aggiorna password solo se fornita
                if (!string.IsNullOrWhiteSpace(request.Password))
                {
                    responsabile.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                }

                // Rimuovi vecchie associazioni
                _context.ResponsabilePuntiVendita.RemoveRange(responsabile.ResponsabilePuntiVendita);

                // Aggiungi nuove associazioni
                foreach (var pvId in request.PuntiVenditaIds)
                {
                    var link = new ResponsabilePuntoVendita
                    {
                        ResponsabileId = responsabile.Id,
                        PuntoVenditaId = pvId,
                        DataAssociazione = DateTime.UtcNow,
                        Principale = request.PuntiVenditaIds.IndexOf(pvId) == 0
                    };
                    _context.ResponsabilePuntiVendita.Add(link);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"[ResponsabiliController] Responsabile '{responsabile.Username}' aggiornato");

                // Ricarica il responsabile con le relazioni per la risposta
                var updated = await _context.Responsabili
                    .Include(r => r.ResponsabilePuntiVendita)
                        .ThenInclude(rp => rp.PuntoVendita)
                    .FirstAsync(r => r.Id == id);

                var response = _mapper.Map<ResponsabileDetailResponse>(updated);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante l'aggiornamento del responsabile.", errore = ex.Message });
            }
        }

        /// <summary>
        /// Elimina un responsabile
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var responsabile = await _context.Responsabili
                    .Include(r => r.ResponsabilePuntiVendita)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (responsabile == null)
                    return NotFound(new { messaggio = "Responsabile non trovato." });

                // Non permettere eliminazione di admin
                if (responsabile.Ruolo == "Admin")
                    return BadRequest(new { messaggio = "Non è possibile eliminare un amministratore." });

                // Elimina prima le associazioni nella junction table
                _context.ResponsabilePuntiVendita.RemoveRange(responsabile.ResponsabilePuntiVendita);
                
                // Poi elimina il responsabile
                _context.Responsabili.Remove(responsabile);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[ResponsabiliController] Responsabile '{responsabile.Username}' eliminato");

                return Ok(new { messaggio = "Responsabile eliminato con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante l'eliminazione del responsabile.", errore = ex.Message });
            }
        }
        /// <summary>
        /// Cambia password per responsabile autenticato
        /// </summary>
        [HttpPost("cambia-password")]
        public async Task<IActionResult> CambiaPassword([FromBody] CambiaPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var responsabileId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
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
                return StatusCode(500, new { success = false, messaggio = "Errore durante il cambio password.", errore = ex.Message });
            }
        }
    }
}
