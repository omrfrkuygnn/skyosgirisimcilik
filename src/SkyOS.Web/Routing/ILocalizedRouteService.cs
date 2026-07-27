using Microsoft.AspNetCore.Http;

namespace SkyOS.Web.Routing;

public interface ILocalizedRouteService
{
    string NormalizeCulture(string? culture);

    string GetCurrentCulture(HttpContext? httpContext = null);

    string Page(string pageKey, string? culture = null, object? queryValues = null);

    string NewsArticle(string slug, string? culture = null);

    string CurrentPageInCulture(string targetCulture, HttpContext? httpContext = null);

    string LocalizeLocalUrl(string? localUrl, string targetCulture);

    string? GetCurrentPageKey(HttpContext? httpContext = null);

    bool IsCurrentPage(string pageKey, HttpContext? httpContext = null);

    bool IsCurrentPage(HttpContext? httpContext = null, params string[] pageKeys);

    string AbsolutePage(HttpContext httpContext, string pageKey, string? culture = null, object? queryValues = null);

    IReadOnlyList<(string Culture, string Url)> GetAlternateAbsoluteUrls(HttpContext httpContext);

    bool TryResolveLocalized(string? culture, string? slug, out LocalizedPageDefinition page);

    bool TryResolveLegacyPath(PathString path, out LocalizedPageDefinition page);

    bool TryResolvePath(string? path, out LocalizedPageDefinition page, out string culture);
}
