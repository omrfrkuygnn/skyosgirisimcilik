namespace SkyOS.Application.DTOs.Common;

/// <summary>
/// Transport-agnostic representation of an outbound e-mail.
/// </summary>
public sealed class EmailMessage
{
    /// <summary>When set, the message is delivered to this address instead of the team inbox.</summary>
    public string? ToEmail { get; init; }

    public string? ToName { get; init; }

    public required string Subject { get; init; }

    public required string HtmlBody { get; init; }

    public string? PlainTextBody { get; init; }

    /// <summary>Optional reply-to; used so the recipient can answer directly.</summary>
    public string? ReplyToAddress { get; init; }

    public string? ReplyToName { get; init; }
}
