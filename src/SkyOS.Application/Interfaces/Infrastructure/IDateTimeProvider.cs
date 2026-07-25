namespace SkyOS.Application.Interfaces.Infrastructure;

/// <summary>
/// Wraps the system clock so time-dependent business rules (spam windows, timestamps)
/// stay deterministic and unit-testable.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
