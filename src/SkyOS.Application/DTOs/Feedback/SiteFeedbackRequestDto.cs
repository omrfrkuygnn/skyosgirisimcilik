using SkyOS.Domain.Enums;

namespace SkyOS.Application.DTOs.Feedback;

public sealed class SiteFeedbackRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public FeedbackCategory Category { get; set; } = FeedbackCategory.Other;

    public string Message { get; set; } = string.Empty;

    public string? Website { get; set; }

    public string? RecaptchaToken { get; set; }

    public string? IpAddress { get; set; }

    public string Culture { get; set; } = "tr";
}
