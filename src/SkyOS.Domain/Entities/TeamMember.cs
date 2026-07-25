using SkyOS.Domain.Common;

namespace SkyOS.Domain.Entities;

public class TeamMember : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? PhotoUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsLeader { get; set; }
}
