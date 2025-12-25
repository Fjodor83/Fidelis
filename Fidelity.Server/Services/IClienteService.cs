using Fidelity.Shared.DTOs;

namespace Fidelity.Server.Services
{
    public interface IClienteService
    {
        Task<List<ClienteResponse>> CercaClientiAsync(string query, string? userRole, int? userPuntoVenditaId);
        Task<List<ClienteResponse>> GetClientiByPuntoVenditaAsync(string? userRole, int? userPuntoVenditaId);
        Task<ClienteResponse?> GetClienteByIdAsync(int id, string? userRole, int? userPuntoVenditaId);
    }
}
