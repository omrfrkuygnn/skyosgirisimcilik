using SkyOS.Domain.Common;

namespace SkyOS.Domain.Entities;

public class Partner : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? Description { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? WebsiteUrl { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>
    /// True only for relationships that are contractually/publicly confirmed
    /// (e.g. HAVELSAN Jet Cube). Unverified names must not claim a formal partnership.
    /// </summary>
    public bool IsVerifiedRelationship { get; set; }
}
