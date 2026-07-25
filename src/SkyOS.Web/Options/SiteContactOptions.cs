namespace SkyOS.Web.Options;

/// <summary>
/// Public-facing contact + brand information, bound from the "SiteContact" section so it can be
/// changed without a redeploy. These values are placeholders until finalised (see appsettings).
/// </summary>
public sealed class SiteContactOptions
{
    public const string SectionName = "SiteContact";

    public string CompanyName { get; set; } = "SkyOS";

    public string TeamName { get; set; } = "ORKA Mühendislik";

    public string Email { get; set; } = string.Empty;

    public string PhonePrimary { get; set; } = string.Empty;

    public string PhoneSecondary { get; set; } = string.Empty;

    /// <summary>Marks that current contact details are placeholders (shows an internal-only note).</summary>
    public bool ContactDetailsArePlaceholder { get; set; } = true;

    public SocialLinks Social { get; set; } = new();

    public sealed class SocialLinks
    {
        public string? LinkedIn { get; set; }

        public string? X { get; set; }

        public string? GitHub { get; set; }

        public string? YouTube { get; set; }

        public string? Instagram { get; set; }
    }
}
