using SkyOS.Web.Routing;

namespace SkyOS.Web.Middleware;

public sealed class LegacyLocalizedRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public LegacyLocalizedRedirectMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ILocalizedRouteService routes)
    {
        var path = context.Request.Path;
        if (ShouldSkip(path) || StartsWithCulturePrefix(path))
        {
            await _next(context);
            return;
        }

        if (routes.TryResolveLegacyPath(path, out var page))
        {
            var target = routes.Page(page.PageKey, "tr", context.Request.Query);
            if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.Redirect(target, permanent: true);
                return;
            }

            // Rewrite POST/PUT in-place so antiforgery cookies and form body are preserved.
            // A 307 redirect would drop the body and surface as HTTP 405 on the localized URL.
            context.Request.Path = new PathString(target);
            await _next(context);
            return;
        }

        await _next(context);
    }

    private static bool StartsWithCulturePrefix(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.StartsWith("/tr", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/en", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/de", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldSkip(PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return value.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/fonts", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/images", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/docs", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/dil", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/lang", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/hata", StringComparison.OrdinalIgnoreCase)
            || Path.HasExtension(value);
    }
}
