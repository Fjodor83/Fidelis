namespace Fidelity.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clienti { get; }
    ICouponRepository Coupons { get; }
    ITransazioneRepository Transazioni { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
