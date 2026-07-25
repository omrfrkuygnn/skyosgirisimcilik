using Mapster;
using SkyOS.Application.DTOs.Partners;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;

namespace SkyOS.Application.Services;

public sealed class PartnerService : IPartnerService
{
    private readonly IUnitOfWork _unitOfWork;

    public PartnerService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<PartnerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var partners = await _unitOfWork.Repository<Partner>().ListAsync(cancellationToken).ConfigureAwait(false);
        return partners
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .Select(p => p.Adapt<PartnerDto>())
            .ToList();
    }

    public async Task<IReadOnlyList<PartnerDto>> GetVerifiedAsync(CancellationToken cancellationToken = default)
    {
        var partners = await _unitOfWork.Repository<Partner>()
            .ListAsync(p => p.IsVerifiedRelationship, cancellationToken)
            .ConfigureAwait(false);

        return partners
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Name)
            .Select(p => p.Adapt<PartnerDto>())
            .ToList();
    }
}
