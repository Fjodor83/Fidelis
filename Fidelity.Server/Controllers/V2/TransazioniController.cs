using Fidelity.Application.Transazioni.Commands.RegistraTransazione;
using Fidelity.Application.Transazioni.Queries.GetStoricoTransazioni;
using Fidelity.Application.Clienti.Queries.GetCliente;
using Fidelity.Application.DTOs;
using Fidelity.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;

namespace Fidelity.Server.Controllers.V2;

[ApiController]
[Route("api/v2/transazioni")]
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
    [HttpPost("assegna")]
    public async Task<ActionResult<TransazioneDto>> AssegnaPunti([FromBody] AssegnaPuntiRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;
            int puntoVenditaId = !string.IsNullOrEmpty(puntoVenditaIdClaim) ? int.Parse(puntoVenditaIdClaim) : 0;

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

            return Ok(_mapper.Map<TransazioneDto>(result.Data));
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
    /// Ottieni storico transazioni per cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<List<TransazioneDto>>> GetUltimiMovimenti(int clienteId)
    {
        try
        {
            var query = new GetStoricoTransazioniQuery
            {
                ClienteId = clienteId,
                Limit = 50 // Default limit from client service expectations
            };

            var result = await _sender.Send(query);

            return Ok(_mapper.Map<List<TransazioneDto>>(result));
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
