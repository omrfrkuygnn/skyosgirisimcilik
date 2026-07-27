using Mapster;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;

namespace SkyOS.Application.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditLogService(IUnitOfWork unitOfWork, IDateTimeProvider dateTimeProvider)
    {
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task WriteAsync(AuditLogWriteDto entry, CancellationToken cancellationToken = default)
    {
        var entity = entry.Adapt<AuditLog>();
        entity.CreatedAtUtc = _dateTimeProvider.UtcNow;
        await _unitOfWork.Repository<AuditLog>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AuditLogListItemDto>> ListAsync(int take = 200, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<AuditLog>().ListAsync(cancellationToken).ConfigureAwait(false);
        return items
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .Select(x => x.Adapt<AuditLogListItemDto>())
            .ToList();
    }
}
