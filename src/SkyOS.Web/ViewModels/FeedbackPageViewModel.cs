using SkyOS.Application.DTOs.Feedback;

namespace SkyOS.Web.ViewModels;

public sealed class FeedbackPageViewModel
{
    public SiteFeedbackRequestDto Form { get; set; } = new();

    public bool RecaptchaEnabled { get; init; }

    public string RecaptchaSiteKey { get; init; } = string.Empty;
}
