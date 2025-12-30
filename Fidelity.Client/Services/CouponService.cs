using Fidelity.Client.Services.Interfaces;
using Fidelity.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace Fidelity.Client.Services
{
    public class CouponService : ICouponService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CouponService> _logger;

        public CouponService(
            HttpClient http,
            IMemoryCache cache,
            ILogger<CouponService> logger)
        {
            _http = http;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<CouponDTO>> GetCouponsAsync()
        {
            const string cacheKey = "all_coupons";

            if (_cache.TryGetValue(cacheKey, out List<CouponDTO>? cached))
                return cached!;

            try
            {
                var result = await _http.GetFromJsonAsync<List<CouponDTO>>("api/Coupons")
                    ?? new List<CouponDTO>();

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching coupons");
                return new List<CouponDTO>();
            }
        }

        public async Task<List<CouponDTO>> GetCouponsDisponibiliAsync()
        {
            const string cacheKey = "coupons_disponibili";

            if (_cache.TryGetValue(cacheKey, out List<CouponDTO>? cached))
                return cached!;

            try
            {
                var result = await _http.GetFromJsonAsync<List<CouponDTO>>("api/Coupons/disponibili")
                    ?? new List<CouponDTO>();

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(3));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching disponibili coupons");
                return new List<CouponDTO>();
            }
        }

        public async Task<List<CouponAssegnatoDTO>> GetCouponsClienteAsync(int clienteId)
        {
            var cacheKey = $"coupons_cliente_{clienteId}";

            if (_cache.TryGetValue(cacheKey, out List<CouponAssegnatoDTO>? cached))
                return cached!;

            try
            {
                var result = await _http.GetFromJsonAsync<List<CouponAssegnatoDTO>>(
                    $"api/Coupons/cliente/{clienteId}"
                ) ?? new List<CouponAssegnatoDTO>();

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(2));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cliente coupons");
                return new List<CouponAssegnatoDTO>();
            }
        }

        public async Task<List<CouponAssegnatoDTO>> GetMieiCouponAsync()
        {
            try
            {
                var result = await _http.GetFromJsonAsync<List<CouponAssegnatoDTO>>("api/Coupons/miei-coupon")
                    ?? new List<CouponAssegnatoDTO>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching my coupons");
                return new List<CouponAssegnatoDTO>();
            }
        }

        public async Task<bool> AssegnaCouponAsync(AssegnaCouponRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Coupons/assegna", request);

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCouponsCache();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning coupon");
                return false;
            }
        }

        public async Task<bool> RiscattaCouponAsync(RiscattaCouponRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Coupons/riscatta", request);

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCouponsCache();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming coupon");
                return false;
            }
        }

        public async Task<bool> CreateCouponAsync(CouponRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Coupons", request);

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCouponsCache();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating coupon");
                return false;
            }
        }

        public async Task<bool> UpdateCouponAsync(int id, CouponRequest request)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Coupons/{id}", request);

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCouponsCache();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coupon");
                return false;
            }
        }

        public async Task<bool> DeleteCouponAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Coupons/{id}");

                if (response.IsSuccessStatusCode)
                {
                    InvalidateCouponsCache();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coupon");
                return false;
            }
        }

        private void InvalidateCouponsCache()
        {
            _cache.Remove("all_coupons");
            _cache.Remove("coupons_disponibili");
        }
    }
}
