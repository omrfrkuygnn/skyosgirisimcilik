namespace SkyOS.Domain.Exceptions;

/// <summary>
/// Base type for violations of invariants that live purely inside the domain.
/// Application-level, expected failures should prefer the Result pattern over exceptions.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
