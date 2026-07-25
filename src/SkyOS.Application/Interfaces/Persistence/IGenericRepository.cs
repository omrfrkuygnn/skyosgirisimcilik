using System.Linq.Expressions;
using SkyOS.Domain.Common;

namespace SkyOS.Application.Interfaces.Persistence;

/// <summary>
/// Generic, technology-agnostic repository abstraction. Implemented in Infrastructure over
/// EF Core. Read methods return <see cref="IReadOnlyList{T}"/> so callers cannot mutate the set.
/// </summary>
public interface IGenericRepository<T>
    where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);
}
