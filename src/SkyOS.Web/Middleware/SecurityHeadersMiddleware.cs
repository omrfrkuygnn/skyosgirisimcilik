using System.Security.Cryptography;

namespace SkyOS.Web.Middleware;

/// <summary>
/// Emits strict security response headers on every request, including a per-request CSP nonce
/// that inline &lt;style&gt;/&lt;script&gt; tags reference. No unsafe-inline is permitted.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    public const string NonceItemKey = "csp-nonce";

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var nonce = GenerateNonce();
        context.Items[NonceItemKey] = nonce;

        var headers = context.Response.Headers;

        headers["Content-Security-Policy"] = BuildContentSecurityPolicy(nonce);
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), interest-cohort=()";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";

        // Remove server fingerprinting where possible.
        headers.Remove("X-Powered-By");

        await _next(context).ConfigureAwait(false);
    }

    private string BuildContentSecurityPolicy(string nonce)
    {
        // Google endpoints are whitelisted only to support reCAPTCHA v3.
        const string google = "https://www.google.com https://www.gstatic.com";

        var scriptSrc = $"script-src 'self' 'nonce-{nonce}' {google}";

        // In Development the browser-refresh / hot-reload injects inline scripts; relax just enough locally.
        if (_environment.IsDevelopment())
        {
            scriptSrc += " 'unsafe-inline'";
        }

        return string.Join("; ", new[]
        {
            "default-src 'self'",
            "base-uri 'self'",
            "object-src 'none'",
            "frame-ancestors 'none'",
            "form-action 'self'",
            scriptSrc,
            $"style-src 'self' 'unsafe-inline' https://www.gstatic.com",
            "img-src 'self' data:",
            "font-src 'self'",
            $"connect-src 'self' {google}",
            $"frame-src {google}",
            "upgrade-insecure-requests",
        });
    }

    private static string GenerateNonce()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes);
    }
}
