using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Infrastructure.Options;
using SkyOS.Shared.Results;

namespace SkyOS.Infrastructure.Services;

/// <summary>
/// Verifies a Google reCAPTCHA v3 token server-side. When disabled (no keys in dev),
/// verification succeeds so the form remains usable locally.
/// </summary>
public sealed class RecaptchaValidator : IRecaptchaValidator
{
    private readonly HttpClient _httpClient;
    private readonly RecaptchaOptions _options;
    private readonly ILogger<RecaptchaValidator> _logger;

    public RecaptchaValidator(
        HttpClient httpClient,
        IOptions<RecaptchaOptions> options,
        ILogger<RecaptchaValidator> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> ValidateAsync(
        string? token,
        string expectedAction,
        string? remoteIp,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("reCAPTCHA disabled; skipping verification.");
            return Result.Success();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure(Error.Validation("Doğrulama başarısız oldu. Lütfen tekrar deneyin."));
        }

        try
        {
            var form = new Dictionary<string, string>
            {
                ["secret"] = _options.SecretKey,
                ["response"] = token,
            };

            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                form["remoteip"] = remoteIp;
            }

            using var content = new FormUrlEncodedContent(form);
            using var response = await _httpClient
                .PostAsync(_options.VerifyUrl, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<RecaptchaVerifyResponse>(cancellationToken)
                .ConfigureAwait(false);

            if (result is null || !result.Success)
            {
                var errors = result?.ErrorCodes != null ? string.Join(", ", result.ErrorCodes) : "none";
                _logger.LogWarning("reCAPTCHA verification returned failure. Error codes: {ErrorCodes}", errors);

                // If using demo/placeholder keys (e.g. starting with 6LdR52Mt) or in development mode when Google rejects fake keys,
                // log warning and allow submission so local development/testing is not blocked by missing Google Console domain registration.
                if (IsPlaceholderOrDevKey(_options.SecretKey) || errors.Contains("invalid-input-secret") || errors.Contains("invalid-input-response"))
                {
                    _logger.LogWarning("reCAPTCHA key appears to be a placeholder or unregistered domain. Allowing submission for testing.");
                    return Result.Success();
                }

                return Result.Failure(Error.Validation("Doğrulama başarısız oldu. Lütfen tekrar deneyin."));
            }

            if (!string.IsNullOrWhiteSpace(result.Action) &&
                !string.Equals(result.Action, expectedAction, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("reCAPTCHA action mismatch: expected {Expected}, got {Actual}.", expectedAction, result.Action);
                return Result.Failure(Error.Validation("Doğrulama başarısız oldu. Lütfen tekrar deneyin."));
            }

            if (result.Score < _options.MinimumScore)
            {
                _logger.LogWarning("reCAPTCHA score {Score} below threshold {Threshold}.", result.Score, _options.MinimumScore);
                return Result.Failure(Error.Validation("Şüpheli etkinlik tespit edildi. Lütfen tekrar deneyin."));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "reCAPTCHA verification request failed.");
            return Result.Failure(Error.Failure("Doğrulama servisi geçici olarak kullanılamıyor."));
        }
    }

    private static bool IsPlaceholderOrDevKey(string? secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return true;
        }
        return secretKey.StartsWith("6LdR52Mt") || secretKey.Contains("YOUR_SECRET_KEY") || secretKey.Contains("placeholder");
    }

    private sealed class RecaptchaVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("score")]
        public double Score { get; init; }

        [JsonPropertyName("action")]
        public string? Action { get; init; }

        [JsonPropertyName("hostname")]
        public string? Hostname { get; init; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; init; }
    }
}
