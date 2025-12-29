using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fidelity.Shared.DTOs;
using System.Security.Claims;
using MediatR;
using AutoMapper;
using Fidelity.Application.Transazioni.Commands.RegistraTransazione;
using Fidelity.Application.Transazioni.Queries.GetStoricoTransazioni;
using Fidelity.Application.Clienti.Queries.GetCliente;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Responsabile,Admin")]
    public class TransazioniController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public TransazioniController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
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
                var ruolo = User.FindFirst(ClaimTypes.Role)?.Value;
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;

                int puntoVenditaId = 0;
                if (!string.IsNullOrEmpty(puntoVenditaIdClaim))
                {
                    puntoVenditaId = int.Parse(puntoVenditaIdClaim);
                }

                if (puntoVenditaId == 0)
                {
                    // Fallback or Error? For now let's try to assume they are operating on the client's registration store or just fail.
                    // But actually, the error "Punto vendita 0 non trovato" comes from the Handler.
                    // If we leave it as 0, it fails. 
                    // Let's rely on the claim being present.
                }

                // 1. Find Client by Fidelity Code
                var clientQuery = new GetClienteByCodiceFidelityQuery { CodiceFidelity = request.CodiceFidelity };
                var clientResult = await _sender.Send(clientQuery);

                if (!clientResult.Succeeded || clientResult.Data == null)
                    return NotFound(new { messaggio = "Cliente non trovato con questo codice fedeltà." });

                // 2. Register Transaction
                var command = new RegistraTransazioneCommand
                {
                    ClienteId = clientResult.Data.Id,
                    PuntoVenditaId = puntoVenditaId,
                    ResponsabileId = responsabileId,
                    Importo = request.ImportoSpesa,
                    Note = request.Note
                };

                var result = await _sender.Send(command);

                if (!result.Succeeded)
                    return BadRequest(new { messaggio = result.Errors.FirstOrDefault() ?? "Errore durante l'assegnazione" });

                return Ok(_mapper.Map<TransazioneResponse>(result.Data));
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
                var query = new GetClienteByCodiceFidelityQuery { CodiceFidelity = codiceFidelity };
                var result = await _sender.Send(query);

                if (!result.Succeeded)
                    return NotFound(new { messaggio = "Cliente non trovato." });

                return Ok(_mapper.Map<ClienteDettaglioResponse>(result.Data));
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

                var query = new GetStoricoTransazioniQuery
                {
                    PuntoVenditaId = puntoVenditaId > 0 ? puntoVenditaId : null,
                    ClienteId = clienteId,
                    DataInizio = dataInizio,
                    DataFine = dataFine,
                    Limit = limit
                };

                var result = await _sender.Send(query);

                return Ok(_mapper.Map<List<TransazioneResponse>>(result));
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
