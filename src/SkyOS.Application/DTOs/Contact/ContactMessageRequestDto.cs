using SkyOS.Domain.Enums;

namespace SkyOS.Application.DTOs.Contact;

/// <summary>
/// Inbound contact-form payload. The human-facing fields are validated by FluentValidation;
/// the anti-bot and context fields are consumed by the service, never shown or trusted from the client blindly.
/// </summary>
public sealed class ContactMessageRequestDto
{
    public string FullName { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PhoneCountryCode { get; set; } = "+90";

    public string? Phone { get; set; }

    public InterestType InterestType { get; set; } = InterestType.Diger;

    public string Message { get; set; } = string.Empty;

    // ---- Anti-bot / context (not part of the visible, validated form model) ----

    /// <summary>Honeypot field. Must be empty for a genuine human submission.</summary>
    public string? Website { get; set; }

    public string? RecaptchaToken { get; set; }

    /// <summary>Set server-side from the request; never bound from user input.</summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Two-letter ISO culture code of the visitor's UI language (e.g. "tr" or "en").
    /// Set server-side from the active request culture; never trusted from the client.
    /// </summary>
    public string Culture { get; set; } = "tr";
}
