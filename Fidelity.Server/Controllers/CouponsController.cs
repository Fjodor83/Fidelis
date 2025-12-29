using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fidelity.Shared.DTOs;
using System.Security.Claims;
using MediatR;
using AutoMapper;
using Fidelity.Application.Coupons.Queries.GetCoupons;
using Fidelity.Application.Coupons.Queries.GetCouponById;
using Fidelity.Application.Coupons.Queries.GetCouponsByCliente;
using Fidelity.Application.Coupons.Commands.CreaCoupon;
using Fidelity.Application.Coupons.Commands.UpdateCoupon;
using Fidelity.Application.Coupons.Commands.DeleteCoupon;
using Fidelity.Application.Coupons.Commands.AssegnaCoupon;
using Fidelity.Application.Coupons.Commands.RiscattaCoupon;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CouponsController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IMapper _mapper;

        public CouponsController(ISender sender, IMapper mapper)
        {
            _sender = sender;
            _mapper = mapper;
        }

        // GET: api/Coupons
        [HttpGet]
        public async Task<ActionResult<List<CouponDTO>>> GetCoupons()
        {
            var result = await _sender.Send(new GetCouponsQuery());
            return Ok(_mapper.Map<List<CouponDTO>>(result));
        }

        // GET: api/Coupons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CouponDTO>> GetCoupon(int id)
        {
            var result = await _sender.Send(new GetCouponByIdQuery(id));
            
            if (!result.Succeeded) return NotFound();
            
            return Ok(_mapper.Map<CouponDTO>(result.Data));
        }

        // POST: api/Coupons
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CouponDTO>> PostCoupon(CouponRequest request)
        {
            try
            {
                var command = new CreaCouponCommand
                {
                    Codice = request.Codice,
                    Titolo = request.Titolo,
                    Descrizione = request.Descrizione,
                    ValoreSconto = request.ValoreSconto,
                    TipoSconto = request.TipoSconto,
                    DataInizio = request.DataInizio,
                    DataScadenza = request.DataScadenza,
                    Attivo = request.Attivo,
                    ImportoMinimoOrdine = request.ImportoMinimoOrdine,
                    LimiteUtilizzoPerCliente = request.LimiteUtilizzoPerCliente
                };

                var result = await _sender.Send(command);

                if (!result.Succeeded) 
                    return BadRequest(new { messaggio = result.Errors.FirstOrDefault() });

                // Fetch created coupon to return full DTO
                var created = await _sender.Send(new GetCouponByIdQuery(result.Data));
                var dto = _mapper.Map<CouponDTO>(created.Data);

                return CreatedAtAction("GetCoupon", new { id = dto.Id }, dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { messaggio = ex.Message });
            }
        }

        // PUT: api/Coupons/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCoupon(int id, CouponRequest request)
        {
            try
            {
                var command = new UpdateCouponCommand
                {
                    Id = id,
                    Codice = request.Codice,
                    Titolo = request.Titolo,
                    Descrizione = request.Descrizione,
                    ValoreSconto = request.ValoreSconto,
                    TipoSconto = request.TipoSconto,
                    DataInizio = request.DataInizio,
                    DataScadenza = request.DataScadenza,
                    Attivo = request.Attivo,
                    ImportoMinimoOrdine = request.ImportoMinimoOrdine
                };

                var result = await _sender.Send(command);

                if (!result.Succeeded) 
                {
                    if (result.Errors.Contains("Coupon non trovato")) return NotFound();
                    return BadRequest(new { messaggio = result.Errors.FirstOrDefault() });
                }

                return NoContent();
            }
            catch (ArgumentException ex) { return BadRequest(new { messaggio = ex.Message }); }
        }

        // DELETE: api/Coupons/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var result = await _sender.Send(new DeleteCouponCommand(id));
            if (!result.Succeeded) return NotFound();
            return NoContent();
        }

        // POST: api/Coupons/assegna
        [HttpPost("assegna")]
        public async Task<IActionResult> AssegnaCoupon(AssegnaCouponRequest request)
        {
            try
            {
                var command = new AssegnaCouponCommand
                {
                    CouponId = request.CouponId,
                    ClienteId = request.ClienteId,
                    Motivo = MotivoAssegnazioneDto.Manuale
                };

                var result = await _sender.Send(command);

                if (!result.Succeeded)
                    return BadRequest(new { messaggio = result.Errors.FirstOrDefault() });

                return Ok(new { messaggio = "Coupon assegnato con successo." });
            }
            catch (Exception ex) { return BadRequest(new { messaggio = ex.Message }); }
        }

        // GET: api/Coupons/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<CouponAssegnatoDTO>>> GetCouponsCliente(int clienteId)
        {
            var result = await _sender.Send(new GetCouponsByClienteQuery(clienteId));
            return Ok(_mapper.Map<List<CouponAssegnatoDTO>>(result));
        }

        // POST: api/Coupons/riscatta
        [HttpPost("riscatta")]
        public async Task<IActionResult> RiscattaCoupon(RiscattaCouponRequest request)
        {
            try
            {
                var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var puntoVenditaIdClaim = User.FindFirst("PuntoVenditaId")?.Value;
                int puntoVenditaId = string.IsNullOrEmpty(puntoVenditaIdClaim) ? 0 : int.Parse(puntoVenditaIdClaim);

                var command = new RiscattaCouponCommand
                {
                    CouponAssegnatoId = request.CouponAssegnatoId,
                    ResponsabileId = responsabileId,
                    PuntoVenditaId = puntoVenditaId
                    // ImportoTransazione can be passed if needed, currently not in request object or optional?
                    // Request DTO definition not seen, assuming basic matching.
                };

                var result = await _sender.Send(command);

                if (!result.Succeeded)
                    return BadRequest(new { messaggio = result.Errors.FirstOrDefault() });

                return Ok(new { messaggio = "Coupon riscattato con successo." });
            }
            catch (Exception ex) { return BadRequest(new { messaggio = ex.Message }); }
        }

        // GET: api/Coupons/disponibili
        [HttpGet("disponibili")]
        public async Task<ActionResult<List<CouponDTO>>> GetCouponsDisponibili()
        {
            var result = await _sender.Send(new GetCouponsDisponibiliQuery());
            return Ok(_mapper.Map<List<CouponDTO>>(result));
        }

        // GET: api/Coupons/miei-coupon
        [HttpGet("miei-coupon")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<List<CouponAssegnatoDTO>>> GetMieiCoupon()
        {
            var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (clienteIdClaim == null) return Unauthorized();
            
            var clienteId = int.Parse(clienteIdClaim.Value);
            var result = await _sender.Send(new GetCouponsByClienteQuery(clienteId));
            return Ok(_mapper.Map<List<CouponAssegnatoDTO>>(result));
        }
    }
}    

