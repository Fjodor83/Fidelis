using Fidelity.Domain.Entities;


namespace Fidelity.Application.Common.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<Cliente?> GetByCodiceFidelityAsync(string codice, CancellationToken ct = default);
    Task<List<Cliente>> GetByLivelloAsync(LivelloFedelta livello, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
