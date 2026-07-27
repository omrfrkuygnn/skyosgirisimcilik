namespace SkyOS.Infrastructure.Options;

/// <summary>
/// Google reCAPTCHA v3 settings, bound from the "Recaptcha" section. The secret key must be
/// supplied via user-secrets/environment variables, never committed to appsettings.json.
/// </summary>
public sealed class RecaptchaOptions
{
    public const string SectionName = "Recaptcha";

    /// <summary>When false, verification is skipped entirely.</summary>
    public bool Enabled { get; set; }

    public string SiteKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string VerifyUrl { get; set; } = "https://www.google.com/recaptcha/api/siteverify";

    /// <summary>Minimum v3 score (0.0–1.0) to accept a submission.</summary>
    public double MinimumScore { get; set; } = 0.5;

    public bool HasConfiguredKeys =>
        !string.IsNullOrWhiteSpace(SiteKey) && !string.IsNullOrWhiteSpace(SecretKey);

    public bool UsesPlaceholderKeys =>
        string.IsNullOrWhiteSpace(SecretKey)
        || SecretKey.StartsWith("6LdR52Mt", StringComparison.Ordinal)
        || SecretKey.Contains("YOUR_SECRET_KEY", StringComparison.OrdinalIgnoreCase)
        || SecretKey.Contains("placeholder", StringComparison.OrdinalIgnoreCase);
}
