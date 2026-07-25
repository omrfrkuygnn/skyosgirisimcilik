using Mapster;
using SkyOS.Application.DTOs.Milestones;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;
using SkyOS.Domain.Enums;

namespace SkyOS.Application.Services;

public sealed class MilestoneService : IMilestoneService
{
    private readonly IUnitOfWork _unitOfWork;

    public MilestoneService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<MilestoneDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var milestones = await _unitOfWork.Repository<Milestone>().ListAsync(cancellationToken).ConfigureAwait(false);
        return milestones
            .OrderBy(m => m.DisplayOrder)
            .ThenByDescending(m => m.DateAchieved)
            .Select(m => m.Adapt<MilestoneDto>())
            .ToList();
    }

    public async Task<IReadOnlyList<MilestoneDto>> GetByCategoryAsync(
        MilestoneCategory category,
        CancellationToken cancellationToken = default)
    {
        var milestones = await _unitOfWork.Repository<Milestone>()
            .ListAsync(m => m.Category == category, cancellationToken)
            .ConfigureAwait(false);

        return milestones
            .OrderBy(m => m.DisplayOrder)
            .ThenByDescending(m => m.DateAchieved)
            .Select(m => m.Adapt<MilestoneDto>())
            .ToList();
    }
}
