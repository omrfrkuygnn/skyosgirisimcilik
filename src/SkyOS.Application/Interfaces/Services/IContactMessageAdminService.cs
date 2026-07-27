using SkyOS.Application.DTOs.Admin;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Services;

public interface IContactMessageAdminService
{
    Task<IReadOnlyList<ContactMessageListItemDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<ContactMessageDetailDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
