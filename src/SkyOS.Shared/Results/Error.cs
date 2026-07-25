namespace SkyOS.Shared.Results;

/// <summary>
/// Represents a failure with a stable machine-readable code and a human-readable message.
/// Used by the <see cref="Result"/> pattern so services never throw for expected flow control.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Validation(string message) => new("General.Validation", message);

    public static Error NotFound(string message) => new("General.NotFound", message);

    public static Error Conflict(string message) => new("General.Conflict", message);

    public static Error Failure(string message) => new("General.Failure", message);

    public static Error Unexpected(string message) => new("General.Unexpected", message);

    public static Error TooManyRequests(string message) => new("General.TooManyRequests", message);
}
