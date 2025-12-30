using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;

namespace Fidelity.Infrastructure.Persistence.Repositories;

public class TransazioneRepository : Repository<Transazione>, ITransazioneRepository
{
    public TransazioneRepository(ApplicationDbContext context) : base(context)
    {
    }
}
