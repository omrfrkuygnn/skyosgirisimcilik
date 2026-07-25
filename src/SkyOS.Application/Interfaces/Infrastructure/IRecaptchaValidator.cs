using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Infrastructure;

/// <summary>
/// Server-side verification of a Google reCAPTCHA v3 token. Implemented in Infrastructure
/// via an HttpClient call to Google's siteverify endpoint.
/// </summary>
public interface IRecaptchaValidator
{
    Task<Result> ValidateAsync(
        string? token,
        string expectedAction,
        string? remoteIp,
        CancellationToken cancellationToken = default);
}
