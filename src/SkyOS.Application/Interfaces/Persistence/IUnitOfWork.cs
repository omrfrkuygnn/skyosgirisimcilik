using SkyOS.Domain.Common;

namespace SkyOS.Application.Interfaces.Persistence;

/// <summary>
/// Coordinates repositories over a single database transaction/context and commits atomically.
/// Controllers never touch the DbContext directly — they go through services -> unit of work.
/// </summary>
public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>()
        where T : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
