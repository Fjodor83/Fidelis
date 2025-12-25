using Fidelity.Shared.DTOs;
using Fidelity.Shared.Models;

namespace Fidelity.Server.Services
{
    public interface ICouponService
    {
        Task<List<CouponDTO>> GetAllCouponsAsync();
        Task<CouponDTO?> GetCouponAsync(int id);
        Task<List<CouponDTO>> GetCouponsDisponibiliAsync();
        Task<CouponDTO> CreateCouponAsync(CouponRequest request);
        Task<CouponDTO?> UpdateCouponAsync(int id, CouponRequest request);
        Task DeleteCouponAsync(int id);
        Task AssegnaCouponAsync(int couponId, int clienteId);
        Task<List<CouponAssegnatoDTO>> GetCouponsClienteAsync(int clienteId);
        Task RiscattaCouponAsync(int couponAssegnatoId);
    }
}
