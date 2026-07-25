using SkyOS.Domain.Enums;

namespace SkyOS.Application.DTOs.Milestones;

public sealed class MilestoneDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTime? DateAchieved { get; init; }

    public MilestoneCategory Category { get; init; }

    public int DisplayOrder { get; init; }
}
