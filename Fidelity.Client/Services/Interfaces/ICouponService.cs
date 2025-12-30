// Fidelity.Client/Services/CouponService.cs
using System.Net.Http.Json;
using Fidelity.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Fidelity.Client.Services.Interfaces;

public interface ICouponService
{
    Task<List<CouponDTO>> GetCouponsAsync();
    Task<List<CouponDTO>> GetCouponsDisponibiliAsync();
    Task<List<CouponAssegnatoDTO>> GetCouponsClienteAsync(int clienteId);
    Task<List<CouponAssegnatoDTO>> GetMieiCouponAsync();
    Task<bool> AssegnaCouponAsync(AssegnaCouponRequest request);
    Task<bool> RiscattaCouponAsync(RiscattaCouponRequest request);
    Task<bool> CreateCouponAsync(CouponRequest request);
    Task<bool> UpdateCouponAsync(int id, CouponRequest request);
    Task<bool> DeleteCouponAsync(int id);
}

