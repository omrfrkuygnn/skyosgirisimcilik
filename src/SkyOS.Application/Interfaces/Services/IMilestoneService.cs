using SkyOS.Application.DTOs.Milestones;
using SkyOS.Domain.Enums;

namespace SkyOS.Application.Interfaces.Services;

public interface IMilestoneService
{
    Task<IReadOnlyList<MilestoneDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MilestoneDto>> GetByCategoryAsync(
        MilestoneCategory category,
        CancellationToken cancellationToken = default);
}
