using Microsoft.EntityFrameworkCore;
using Fidelity.Infrastructure.Persistence;
using Fidelity.Domain.Entities;

namespace Fidelity.Server.Repositories;

public class ClienteRepository : Repository<Cliente>, IClienteRepository
{
    public ClienteRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Cliente?> GetByCodiceFidelityAsync(string codice)
    {
        return await _dbSet
            .Include(c => c.PuntoVenditaRegistrazione)
            .FirstOrDefaultAsync(c => c.CodiceFidelity == codice);
    }

    public async Task<Cliente?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(c => c.PuntoVenditaRegistrazione)
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<List<Cliente>> SearchAsync(string query, int? puntoVenditaId = null)
    {
        var q = _dbSet.Include(c => c.PuntoVenditaRegistrazione).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            q = q.Where(c => 
                c.Nome.Contains(query) || 
                c.Cognome.Contains(query) || 
                c.Email.Contains(query) || 
                c.CodiceFidelity.Contains(query));
        }

        if (puntoVenditaId.HasValue)
        {
            q = q.Where(c => c.PuntoVenditaRegistrazioneId == puntoVenditaId.Value);
        }

        return await q.ToListAsync();
    }
}
