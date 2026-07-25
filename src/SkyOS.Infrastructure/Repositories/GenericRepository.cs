using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Domain.Common;
using SkyOS.Infrastructure.Persistence;

namespace SkyOS.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IGenericRepository{T}"/>. Read queries use
/// <c>AsNoTracking</c> for performance; writes are staged and committed by the unit of work.
/// All querying is LINQ — no raw SQL string concatenation anywhere (SQL-injection safe).
/// </summary>
public class GenericRepository<T> : IGenericRepository<T>
    where T : BaseEntity
{
    private readonly SkyOSDbContext _context;
    private readonly DbSet<T> _set;

    public GenericRepository(SkyOSDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _set.FindAsync([id], cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null
            ? await _set.CountAsync(cancellationToken).ConfigureAwait(false)
            : await _set.CountAsync(predicate, cancellationToken).ConfigureAwait(false);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken).ConfigureAwait(false);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
