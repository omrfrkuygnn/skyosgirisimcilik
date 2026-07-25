using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Domain.Common;
using SkyOS.Infrastructure.Persistence;

namespace SkyOS.Infrastructure.Repositories;

/// <summary>
/// Scoped unit of work wrapping a single <see cref="SkyOSDbContext"/>. Repositories are
/// cached per entity type so callers within one request share the same tracked context.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SkyOSDbContext _context;
    private readonly Dictionary<Type, object> _repositories = [];

    public UnitOfWork(SkyOSDbContext context) => _context = context;

    public IGenericRepository<T> Repository<T>()
        where T : BaseEntity
    {
        if (_repositories.TryGetValue(typeof(T), out var existing))
        {
            return (IGenericRepository<T>)existing;
        }

        var repository = new GenericRepository<T>(_context);
        _repositories[typeof(T)] = repository;
        return repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
