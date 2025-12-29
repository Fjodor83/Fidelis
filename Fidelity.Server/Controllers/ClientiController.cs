using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fidelity.Shared.DTOs;
using System.Security.Claims;
using MediatR;
using AutoMapper;
using Fidelity.Application.Clienti.Queries.GetClienti;
using Fidelity.Application.Clienti.Queries.GetCliente;
using Fidelity.Application.DTOs;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientiController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public ClientiController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
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
                var isStoreUser = User.IsInRole("Responsabile") || User.IsInRole("Operatore");

                var result = await _sender.Send(new GetClientiQuery 
                { 
                    SearchTerm = query, 
                    PuntoVenditaId = isStoreUser ? puntoVenditaId : null 
                });
                
                return Ok(_mapper.Map<List<ClienteResponse>>(result));
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
                int? puntoVenditaId = string.IsNullOrEmpty(puntoVenditaIdClaim) ? null : int.Parse(puntoVenditaIdClaim);
                var isStoreUser = User.IsInRole("Responsabile") || User.IsInRole("Operatore");
                
                var result = await _sender.Send(new GetClientiQuery 
                { 
                    PuntoVenditaId = isStoreUser ? puntoVenditaId : null,
                    SoloAttivi = true
                });
                
                return Ok(_mapper.Map<List<ClienteResponse>>(result));
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
                int? puntoVenditaId = string.IsNullOrEmpty(puntoVenditaIdClaim) ? null : int.Parse(puntoVenditaIdClaim);
                var isStoreUser = User.IsInRole("Responsabile") || User.IsInRole("Operatore");

                var query = new GetClienteQuery 
                { 
                    ClienteId = id,
                    PuntoVenditaId = isStoreUser ? puntoVenditaId : null
                };

                var result = await _sender.Send(query);
                
                if (!result.Succeeded) return NotFound(new { messaggio = result.Errors.FirstOrDefault() ?? "Cliente non trovato" });
                
                return Ok(_mapper.Map<ClienteResponse>(result.Data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { messaggio = "Errore durante il caricamento del cliente.", errore = ex.Message });
            }
        }
    }
}
