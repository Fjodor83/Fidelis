using Fidelity.Domain.Common;
using System.Linq.Expressions;

namespace Fidelity.Application.Common.Interfaces;

/// <summary>
/// Generic Repository Pattern - ISO 25000: Testability
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<List<T>> FindAsync(Domain.Specifications.ISpecification<T> specification, CancellationToken ct = default);
}
