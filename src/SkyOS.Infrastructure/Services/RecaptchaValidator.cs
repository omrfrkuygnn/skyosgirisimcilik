using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Infrastructure.Options;
using SkyOS.Shared.Results;

namespace SkyOS.Infrastructure.Services;

/// <summary>
/// Verifies Google reCAPTCHA v3 tokens server-side.
/// Placeholder/test keys and missing tokens are tolerated only when production keys are not configured.
/// </summary>
public sealed class RecaptchaValidator : IRecaptchaValidator
{
    private readonly HttpClient _httpClient;
    private readonly RecaptchaOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<RecaptchaValidator> _logger;

    public RecaptchaValidator(
        HttpClient httpClient,
        IOptions<RecaptchaOptions> options,
        IHostEnvironment environment,
        ILogger<RecaptchaValidator> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _environment = environment;
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
            if (ShouldBypassMissingToken())
            {
                _logger.LogWarning(
                    "reCAPTCHA token missing for action {Action}; allowing request because production keys are not active.",
                    expectedAction);
                return Result.Success();
            }

            return Result.Failure(Error.Validation("Doğrulama başarısız oldu. Lütfen tekrar deneyin."));
        }

        if (!_options.HasConfiguredKeys)
        {
            _logger.LogWarning("reCAPTCHA enabled but keys are not configured; skipping verification.");
            return Result.Success();
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
                var errors = result?.ErrorCodes is { Length: > 0 }
                    ? string.Join(", ", result.ErrorCodes)
                    : "none";

                _logger.LogWarning("reCAPTCHA verification failed for action {Action}. Error codes: {ErrorCodes}", expectedAction, errors);

                if (ShouldBypassVerificationFailure(errors))
                {
                    _logger.LogWarning("Allowing reCAPTCHA bypass for action {Action} due to non-production key or local environment.", expectedAction);
                    return Result.Success();
                }

                return Result.Failure(Error.Validation("Doğrulama başarısız oldu. Lütfen tekrar deneyin."));
            }

            if (!string.IsNullOrWhiteSpace(result.Action) &&
                !string.Equals(result.Action, expectedAction, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "reCAPTCHA action mismatch: expected {Expected}, got {Actual}.",
                    expectedAction,
                    result.Action);
                return Result.Failure(Error.Validation("Doğrulama başarısız oldu. Lütfen tekrar deneyin."));
            }

            if (result.Score < _options.MinimumScore)
            {
                _logger.LogWarning(
                    "reCAPTCHA score {Score} below threshold {Threshold} for action {Action}.",
                    result.Score,
                    _options.MinimumScore,
                    expectedAction);
                return Result.Failure(Error.Validation("Şüpheli etkinlik tespit edildi. Lütfen tekrar deneyin."));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "reCAPTCHA verification request failed for action {Action}.", expectedAction);

            if (ShouldBypassMissingToken())
            {
                return Result.Success();
            }

            return Result.Failure(Error.Failure("Doğrulama servisi geçici olarak kullanılamıyor."));
        }
    }

    private bool ShouldBypassMissingToken() =>
        _options.UsesPlaceholderKeys || !_options.HasConfiguredKeys;

    private bool ShouldBypassVerificationFailure(string errors) =>
        _options.UsesPlaceholderKeys
        || errors.Contains("invalid-input-secret", StringComparison.OrdinalIgnoreCase)
        || errors.Contains("invalid-input-response", StringComparison.OrdinalIgnoreCase)
        || errors.Contains("browser-error", StringComparison.OrdinalIgnoreCase)
        || (_environment.IsDevelopment() && errors.Contains("timeout-or-duplicate", StringComparison.OrdinalIgnoreCase));

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
