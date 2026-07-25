using SkyOS.Web.Middleware;

namespace SkyOS.Web.Extensions;

public static class HttpContextExtensions
{
    /// <summary>Returns the CSP nonce generated for the current request (empty if none).</summary>
    public static string GetCspNonce(this HttpContext context) =>
        context.Items.TryGetValue(SecurityHeadersMiddleware.NonceItemKey, out var value) && value is string nonce
            ? nonce
            : string.Empty;
}
