namespace SkyOS.Application.DTOs.Common;

/// <summary>
/// Transport-agnostic representation of an outbound e-mail.
/// </summary>
public sealed class EmailMessage
{
    public required string Subject { get; init; }

    public required string HtmlBody { get; init; }

    public string? PlainTextBody { get; init; }

    /// <summary>Optional reply-to; used so the team can answer the visitor directly.</summary>
    public string? ReplyToAddress { get; init; }

    public string? ReplyToName { get; init; }
}
