using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fidelity.Shared.DTOs;
using System.Security.Claims;
using Fidelity.Server.Services;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Responsabile,Admin")]
    public class TransazioniController : ControllerBase
    {
        private readonly ITransazioneService _transazioneService;

        public TransazioniController(ITransazioneService transazioneService)
        {
            _transazioneService = transazioneService;
        }

        /// <summary>
        /// Assegna punti a un cliente in base alla spesa
        /// </summary>
        [HttpPost("assegna-punti")]
        public async Task<ActionResult<TransazioneResponse>> AssegnaPunti(
            [FromBody] AssegnaPuntiRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var responsabileUsername = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;

                int puntoVenditaId = ruolo == "Admin"
                    ? 0 
                    : int.Parse(puntoVenditaIdClaim!);

                var response = await _transazioneService.AssegnaPuntiAsync(request, puntoVenditaId, responsabileId, responsabileUsername);

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { messaggio = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { messaggio = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    messaggio = "Errore durante l'assegnazione dei punti.",
                    errore = ex.Message
                });
            }
        }

        /// <summary>
        /// Ottieni dettagli cliente con ultime transazioni
        /// </summary>
        [HttpGet("cliente/{codiceFidelity}")]
        public async Task<ActionResult<ClienteDettaglioResponse>> GetClienteDettaglio(
            string codiceFidelity)
        {
            try
            {
                var response = await _transazioneService.GetClienteDettaglioAsync(codiceFidelity);

                if (response == null)
                    return NotFound(new { messaggio = "Cliente non trovato." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    messaggio = "Errore durante il caricamento dei dettagli cliente.",
                    errore = ex.Message
                });
            }
        }

        /// <summary>
        /// Ottieni storico transazioni
        /// </summary>
        [HttpGet("storico")]
        public async Task<ActionResult<List<TransazioneResponse>>> GetStorico(
            [FromQuery] int? clienteId = null,
            [FromQuery] DateTime? dataInizio = null,
            [FromQuery] DateTime? dataFine = null,
            [FromQuery] int limit = 50)
        {
            try
            {
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;

                int puntoVenditaId = (ruolo != "Admin" && !string.IsNullOrEmpty(puntoVenditaIdClaim))
                    ? int.Parse(puntoVenditaIdClaim) 
                    : 0;

                var result = await _transazioneService.GetStoricoAsync(puntoVenditaId, clienteId, dataInizio, dataFine, limit);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    messaggio = "Errore durante il caricamento dello storico.",
                    errore = ex.Message
                });
            }
        }
    }
}
