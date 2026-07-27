using SkyOS.Application.DTOs.Admin;

namespace SkyOS.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task WriteAsync(AuditLogWriteDto entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogListItemDto>> ListAsync(int take = 200, CancellationToken cancellationToken = default);
}
