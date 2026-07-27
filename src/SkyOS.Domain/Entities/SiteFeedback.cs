using SkyOS.Domain.Common;
using SkyOS.Domain.Enums;

namespace SkyOS.Domain.Entities;

public class SiteFeedback : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public FeedbackCategory Category { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public string? IpAddress { get; set; }

    public string Culture { get; set; } = "tr";
}
