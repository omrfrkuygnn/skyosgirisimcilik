using Microsoft.AspNetCore.Mvc.Rendering;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Domain.Enums;

namespace SkyOS.Web.ViewModels;

/// <summary>
/// Wraps the validated <see cref="ContactMessageRequestDto"/> together with view-only data
/// (reCAPTCHA site key, interest options). The form binds to <see cref="Form"/> so FluentValidation
/// auto-validation and client-side adapters operate on the DTO directly.
/// </summary>
public sealed class ContactPageViewModel
{
    public ContactMessageRequestDto Form { get; set; } = new();

    public bool RecaptchaEnabled { get; init; }

    public string RecaptchaSiteKey { get; init; } = string.Empty;

    public IReadOnlyList<SelectListItem> InterestOptions { get; init; } = [];

    public IReadOnlyList<SelectListItem> CountryCodeOptions { get; init; } = [];

    /// <summary>When true the form is pre-set to the "Investor" interest (Section 7.7 variant).</summary>
    public bool InvestorVariant { get; init; }
}
