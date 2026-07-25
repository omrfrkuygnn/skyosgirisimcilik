using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Enums;
using SkyOS.Shared.Constants;
using SkyOS.Shared.Localization;
using SkyOS.Web.Helpers;
using SkyOS.Web.ViewModels;

namespace SkyOS.Web.Controllers;

public sealed class ContactController : Controller
{
    private readonly IContactMessageService _contactMessageService;
    private readonly IConfiguration _configuration;
    private readonly IAppLocalizer _L;

    public ContactController(
        IContactMessageService contactMessageService,
        IConfiguration configuration,
        IAppLocalizer localizer)
    {
        _contactMessageService = contactMessageService;
        _configuration = configuration;
        _L = localizer;
    }

    [HttpGet("iletisim")]
    public IActionResult Index([FromQuery] string? ilgi = null)
    {
        var investor = string.Equals(ilgi, "yatirimci", StringComparison.OrdinalIgnoreCase);
        var form = new ContactMessageRequestDto
        {
            InterestType = investor ? InterestType.Yatirimci : InterestType.Diger,
        };
        return View(BuildViewModel(form, investor));
    }

    [HttpPost("iletisim")]
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
        form.Culture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var result = await _contactMessageService.SubmitAsync(form, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return View(BuildViewModel(form, form.InterestType == InterestType.Yatirimci));
        }

        return RedirectToAction(nameof(Success));
    }

    [HttpGet("iletisim/tesekkurler")]
    public IActionResult Success() => View();

    private ContactPageViewModel BuildViewModel(ContactMessageRequestDto form, bool investorVariant) => new()
    {
        Form = form,
        RecaptchaEnabled = _configuration.GetValue<bool>("Recaptcha:Enabled"),
        RecaptchaSiteKey = _configuration.GetValue<string>("Recaptcha:SiteKey") ?? string.Empty,
        InterestOptions = DisplayNames.InterestSelectList(_L, form.InterestType),
        CountryCodeOptions = DisplayNames.CountryCodeSelectList(form.PhoneCountryCode),
        InvestorVariant = investorVariant,
    };
}
