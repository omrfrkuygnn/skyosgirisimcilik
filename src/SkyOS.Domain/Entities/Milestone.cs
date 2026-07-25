using SkyOS.Domain.Common;
using SkyOS.Domain.Enums;

namespace SkyOS.Domain.Entities;

public class Milestone : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? DateAchieved { get; set; }

    public MilestoneCategory Category { get; set; }

    public int DisplayOrder { get; set; }
}
