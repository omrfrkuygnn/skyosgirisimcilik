namespace SkyOS.Infrastructure.Options;

public sealed class BackofficeOptions
{
    public const string SectionName = "Backoffice";

    public string DefaultAdminEmail { get; set; } = "admin@skyos.local";

    public string DefaultAdminPassword { get; set; } = "ChangeMe!2026";

    public string DefaultAdminDisplayName { get; set; } = "SkyOS Admin";
}
