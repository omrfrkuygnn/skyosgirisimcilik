using SkyOS.Application.DTOs.Partners;

namespace SkyOS.Application.Interfaces.Services;

public interface IPartnerService
{
    Task<IReadOnlyList<PartnerDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerDto>> GetVerifiedAsync(CancellationToken cancellationToken = default);
}
