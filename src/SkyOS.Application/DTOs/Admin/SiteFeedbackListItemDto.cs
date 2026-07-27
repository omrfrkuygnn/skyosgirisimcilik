using SkyOS.Domain.Enums;

namespace SkyOS.Application.DTOs.Admin;

public sealed class SiteFeedbackListItemDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public FeedbackCategory Category { get; set; }

    public string MessagePreview { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public string Culture { get; set; } = "tr";

    public DateTime CreatedAtUtc { get; set; }
}
