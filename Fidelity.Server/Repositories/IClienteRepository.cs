using Fidelity.Shared.Models;

namespace Fidelity.Server.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByCodiceFidelityAsync(string codice);
    Task<Cliente?> GetByEmailAsync(string email);
    Task<List<Cliente>> SearchAsync(string query, int? puntoVenditaId = null);
}
