namespace SkyOS.Infrastructure.Options;

/// <summary>
/// SMTP transport settings, bound from the "Smtp" section. The password must NOT live in
/// appsettings.json — provide it via user-secrets (dev) or environment variables (prod).
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool UseStartTls { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "SkyOS";
}
