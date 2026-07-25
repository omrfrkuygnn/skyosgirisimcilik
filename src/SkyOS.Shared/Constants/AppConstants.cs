namespace SkyOS.Shared.Constants;

/// <summary>
/// Solution-wide constant values. Keeping these in one place avoids magic strings
/// leaking across layers (rate-limiter policy names, cache keys, config section names).
/// </summary>
public static class AppConstants
{
    public static class RateLimiting
    {
        /// <summary>Fixed-window policy name applied to the public contact form endpoint.</summary>
        public const string ContactFormPolicy = "contact-form";

        public const int ContactFormPermitPerWindow = 5;

        public const int ContactFormWindowSeconds = 60;
    }

    public static class ConfigSections
    {
        public const string Smtp = "Smtp";
        public const string Recaptcha = "Recaptcha";
        public const string SiteContact = "SiteContact";
        public const string Database = "Database";
    }

    public static class Cache
    {
        /// <summary>One year in seconds — used for immutable, hashed static assets.</summary>
        public const int StaticAssetMaxAgeSeconds = 31_536_000;
    }

    public static class Honeypot
    {
        /// <summary>Hidden field name; a non-empty value indicates an automated submission.</summary>
        public const string FieldName = "Website";
    }
}
