using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fidelity.Shared.DTOs;
using System.Security.Claims;
using Fidelity.Server.Services;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientiController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientiController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        /// <summary>
        /// Cerca un cliente per codice fedeltà o email
        /// </summary>
        [HttpGet("cerca")]
        public async Task<ActionResult<List<ClienteResponse>>> CercaCliente([FromQuery] string query)
        {
            try
            {
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;
                int? puntoVenditaId = string.IsNullOrEmpty(puntoVenditaIdClaim) ? null : int.Parse(puntoVenditaIdClaim);
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _clienteService.CercaClientiAsync(query, ruolo, puntoVenditaId);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(new { messaggio = ex.Message }); }
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
                int? puntoVenditaId = string.IsNullOrEmpty(puntoVenditaIdClaim) ? null : int.Parse(puntoVenditaIdClaim);
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;

                var result = await _clienteService.GetClientiByPuntoVenditaAsync(ruolo, puntoVenditaId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return BadRequest(new { messaggio = ex.Message }); }
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
                int? puntoVenditaId = string.IsNullOrEmpty(puntoVenditaIdClaim) ? null : int.Parse(puntoVenditaIdClaim);
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;

                var cliente = await _clienteService.GetClienteByIdAsync(id, ruolo, puntoVenditaId);
                if (cliente == null) return NotFound(new { messaggio = "Cliente non trovato." });
                
                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il caricamento del cliente.", errore = ex.Message });
            }
        }
    }
}
