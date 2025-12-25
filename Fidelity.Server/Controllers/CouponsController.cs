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
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // GET: api/Coupons
        [HttpGet]
        public async Task<ActionResult<List<CouponDTO>>> GetCoupons()
        {
            var coupons = await _couponService.GetAllCouponsAsync();
            return Ok(coupons);
        }

        // GET: api/Coupons/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CouponDTO>> GetCoupon(int id)
        {
            var coupon = await _couponService.GetCouponAsync(id);
            if (coupon == null) return NotFound();
            return Ok(coupon);
        }

        // POST: api/Coupons
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CouponDTO>> PostCoupon(CouponRequest request)
        {
            try
            {
                var couponDTO = await _couponService.CreateCouponAsync(request);
                return CreatedAtAction("GetCoupon", new { id = couponDTO.Id }, couponDTO);
            }
            catch (ArgumentException ex)
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
                var updated = await _couponService.UpdateCouponAsync(id, request);
                if (updated == null) return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex) { return BadRequest(new { messaggio = ex.Message }); }
        }

        // DELETE: api/Coupons/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            await _couponService.DeleteCouponAsync(id);
            return NoContent();
        }

        // POST: api/Coupons/assegna
        [HttpPost("assegna")]
        public async Task<IActionResult> AssegnaCoupon(AssegnaCouponRequest request)
        {
            try
            {
                await _couponService.AssegnaCouponAsync(request.CouponId, request.ClienteId);
                return Ok(new { messaggio = "Coupon assegnato con successo." });
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // GET: api/Coupons/cliente/{clienteId}
        [HttpGet("cliente/{clienteId}")]
        public async Task<ActionResult<List<CouponAssegnatoDTO>>> GetCouponsCliente(int clienteId)
        {
            var coupons = await _couponService.GetCouponsClienteAsync(clienteId);
            return Ok(coupons);
        }

        // POST: api/Coupons/riscatta
        [HttpPost("riscatta")]
        public async Task<IActionResult> RiscattaCoupon(RiscattaCouponRequest request)
        {
            try
            {
                await _couponService.RiscattaCouponAsync(request.CouponAssegnatoId);
                return Ok(new { messaggio = "Coupon riscattato con successo." });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // GET: api/Coupons/disponibili
        [HttpGet("disponibili")]
        public async Task<ActionResult<List<CouponDTO>>> GetCouponsDisponibili()
        {
            var coupons = await _couponService.GetCouponsDisponibiliAsync();
            return Ok(coupons);
        }

        // GET: api/Coupons/miei-coupon
        [HttpGet("miei-coupon")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<List<CouponAssegnatoDTO>>> GetMieiCoupon()
        {
            var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (clienteIdClaim == null) return Unauthorized();
            
            var clienteId = int.Parse(clienteIdClaim.Value);
            var coupons = await _couponService.GetCouponsClienteAsync(clienteId);
            return Ok(coupons);
        }
    }
}    

