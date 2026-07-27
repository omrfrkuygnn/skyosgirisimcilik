using SkyOS.Domain.Enums;

namespace SkyOS.Application.DTOs.Admin;

public sealed class TeamMemberUpsertDto
{
    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? PhotoUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsLeader { get; set; }
}

public sealed class PartnerUpsertDto
{
    public string Name { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? Description { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? SupportLetterUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsVerifiedRelationship { get; set; }
}

public sealed class MilestoneUpsertDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? DateAchieved { get; set; }

    public MilestoneCategory Category { get; set; } = MilestoneCategory.Kurumsal;

    public int DisplayOrder { get; set; }
}

public sealed class NewsItemUpsertDto
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime PublishedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsPublished { get; set; }
}
