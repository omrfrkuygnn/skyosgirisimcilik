using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Enums;
using SkyOS.Infrastructure.Options;
using SkyOS.Shared.Constants;
using SkyOS.Shared.Localization;
using SkyOS.Web.Helpers;
using SkyOS.Web.Routing;
using SkyOS.Web.ViewModels;

namespace SkyOS.Web.Controllers;

public sealed class ContactController : Controller
{
    private readonly IContactMessageService _contactMessageService;
    private readonly IOptions<RecaptchaOptions> _recaptchaOptions;
    private readonly IAppLocalizer _L;
    private readonly ILocalizedRouteService _routes;

    public ContactController(
        IContactMessageService contactMessageService,
        IOptions<RecaptchaOptions> recaptchaOptions,
        IAppLocalizer localizer,
        ILocalizedRouteService routes)
    {
        _contactMessageService = contactMessageService;
        _recaptchaOptions = recaptchaOptions;
        _L = localizer;
        _routes = routes;
    }

    [HttpGet]
    public IActionResult Index([FromQuery] string? ilgi = null)
    {
        var investor = string.Equals(ilgi, "yatirimci", StringComparison.OrdinalIgnoreCase);
        var form = new ContactMessageRequestDto
        {
            InterestType = investor ? InterestType.Yatirimci : InterestType.Diger,
        };
        return View(BuildViewModel(form, investor));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(AppConstants.RateLimiting.ContactFormPolicy)]
    public async Task<IActionResult> Index(
        [Bind(Prefix = nameof(ContactPageViewModel.Form))] ContactMessageRequestDto form,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(BuildViewModel(form, form.InterestType == InterestType.Yatirimci));
        }

        // IP and Culture are set server-side and never trusted from the client.
        form.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        form.Culture = _routes.GetCurrentCulture(HttpContext);

        var result = await _contactMessageService.SubmitAsync(form, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(BuildViewModel(form, form.InterestType == InterestType.Yatirimci));
        }

        return Redirect(_routes.Page(SitePageKeys.ContactSuccess));
    }

    [HttpGet]
    public IActionResult Success() => View();

    private ContactPageViewModel BuildViewModel(ContactMessageRequestDto form, bool investorVariant) => new()
    {
        Form = form,
        RecaptchaEnabled = RecaptchaUi.IsEnabled(_recaptchaOptions),
        RecaptchaSiteKey = _recaptchaOptions.Value.SiteKey,
        InterestOptions = DisplayNames.InterestSelectList(_L, form.InterestType),
        CountryCodeOptions = DisplayNames.CountryCodeSelectList(form.PhoneCountryCode),
        InvestorVariant = investorVariant,
    };
}
