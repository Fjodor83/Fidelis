using Fidelity.Application.DTOs;

namespace Fidelity.Client.Services.Interfaces;

public interface IClienteService
{
    Task<List<ClienteDto>> Search(string query);
    Task<ClienteDetailDto> GetById(int id);
    Task<ClienteDetailDto> GetByCodiceFidelity(string codice);
    Task<ClienteDetailDto> GetClienteDettaglioAsync(string clienteId);
}
