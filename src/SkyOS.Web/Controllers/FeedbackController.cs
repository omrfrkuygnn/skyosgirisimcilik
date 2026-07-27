using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SkyOS.Application.DTOs.Feedback;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Infrastructure.Options;
using SkyOS.Shared.Constants;
using SkyOS.Web.Helpers;
using SkyOS.Web.Routing;
using SkyOS.Web.ViewModels;

namespace SkyOS.Web.Controllers;

public sealed class FeedbackController : Controller
{
    private readonly ISiteFeedbackService _feedbackService;
    private readonly ILocalizedRouteService _routes;
    private readonly IOptions<RecaptchaOptions> _recaptchaOptions;

    public FeedbackController(
        ISiteFeedbackService feedbackService,
        ILocalizedRouteService routes,
        IOptions<RecaptchaOptions> recaptchaOptions)
    {
        _feedbackService = feedbackService;
        _routes = routes;
        _recaptchaOptions = recaptchaOptions;
    }

    [HttpGet]
    public IActionResult Index() => View(BuildViewModel(new SiteFeedbackRequestDto()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(AppConstants.RateLimiting.ContactFormPolicy)]
    public async Task<IActionResult> Index(
        [Bind(Prefix = nameof(FeedbackPageViewModel.Form))] SiteFeedbackRequestDto form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildViewModel(form));
        }

        form.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        form.Culture = _routes.GetCurrentCulture(HttpContext);

        var result = await _feedbackService.SubmitAsync(form, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(BuildViewModel(form));
        }

        TempData["FeedbackSuccess"] = true;
        return Redirect(_routes.Page(SitePageKeys.Feedback));
    }

    private FeedbackPageViewModel BuildViewModel(SiteFeedbackRequestDto form) => new()
    {
        Form = form,
        RecaptchaEnabled = RecaptchaUi.IsEnabled(_recaptchaOptions),
        RecaptchaSiteKey = _recaptchaOptions.Value.SiteKey,
    };
}
