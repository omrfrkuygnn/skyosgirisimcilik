namespace SkyOS.Application.Options;

/// <summary>
/// Business configuration for the contact form, bound from the "ContactForm" section.
/// Keeps the notification recipient and spam thresholds out of code.
/// </summary>
public sealed class ContactFormOptions
{
    public const string SectionName = "ContactForm";

    /// <summary>Team inbox that receives a notification when a message is submitted.</summary>
    public string NotificationRecipientEmail { get; set; } = string.Empty;

    public string NotificationRecipientName { get; set; } = "SkyOS";

    /// <summary>Max submissions allowed from a single IP within <see cref="SpamWindowMinutes"/>.</summary>
    public int MaxSubmissionsPerWindow { get; set; } = 5;

    public int SpamWindowMinutes { get; set; } = 10;
}
