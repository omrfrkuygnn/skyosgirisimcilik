using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SkyOS.Web.Localization;

namespace SkyOS.Web.Routing;

public sealed class LocalizedRouteService : ILocalizedRouteService
{
    public const string CultureRouteKey = "culture";
    public const string PageKeyRouteKey = "pageKey";

    private static readonly string[] SupportedCultures = ["tr", "en", "de"];

    private static readonly IReadOnlyDictionary<string, LocalizedPageDefinition> Pages =
        new Dictionary<string, LocalizedPageDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            [SitePageKeys.Home] = new(
                SitePageKeys.Home,
                "Home",
                "Index",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = string.Empty,
                    ["en"] = string.Empty,
                    ["de"] = string.Empty,
                }),
            [SitePageKeys.About] = new(
                SitePageKeys.About,
                "Pages",
                "Hakkimizda",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "hakkimizda",
                    ["en"] = "about",
                    ["de"] = "uber-uns",
                }),
            [SitePageKeys.Product] = new(
                SitePageKeys.Product,
                "Pages",
                "Urun",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "urun",
                    ["en"] = "product",
                    ["de"] = "produkt",
                }),
            [SitePageKeys.UseCases] = new(
                SitePageKeys.UseCases,
                "Pages",
                "KullanimAlanlari",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "kullanim-alanlari",
                    ["en"] = "use-cases",
                    ["de"] = "anwendungsbereiche",
                }),
            [SitePageKeys.Achievements] = new(
                SitePageKeys.Achievements,
                "Pages",
                "Basarilar",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "basarilar",
                    ["en"] = "achievements",
                    ["de"] = "erfolge",
                }),
            [SitePageKeys.Investors] = new(
                SitePageKeys.Investors,
                "Pages",
                "Yatirimcilar",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "yatirimcilar",
                    ["en"] = "investors",
                    ["de"] = "investoren",
                }),
            [SitePageKeys.Partners] = new(
                SitePageKeys.Partners,
                "Partners",
                "Index",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "destekcilerimiz",
                    ["en"] = "partners",
                    ["de"] = "partner",
                }),
            [SitePageKeys.Contact] = new(
                SitePageKeys.Contact,
                "Contact",
                "Index",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "iletisim",
                    ["en"] = "contact",
                    ["de"] = "kontakt",
                }),
            [SitePageKeys.ContactSuccess] = new(
                SitePageKeys.ContactSuccess,
                "Contact",
                "Success",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "iletisim/tesekkurler",
                    ["en"] = "contact/thank-you",
                    ["de"] = "kontakt/danke",
                }),
            [SitePageKeys.Feedback] = new(
                SitePageKeys.Feedback,
                "Feedback",
                "Index",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "geri-bildirim",
                    ["en"] = "feedback",
                    ["de"] = "feedback",
                }),
            [SitePageKeys.News] = new(
                SitePageKeys.News,
                "News",
                "Index",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "haberler",
                    ["en"] = "news",
                    ["de"] = "nachrichten",
                }),
            [SitePageKeys.PrivacyPolicy] = new(
                SitePageKeys.PrivacyPolicy,
                "Pages",
                "GizlilikPolitikasi",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "gizlilik-politikasi",
                    ["en"] = "privacy-policy",
                    ["de"] = "datenschutz",
                }),
            [SitePageKeys.Team] = new(
                SitePageKeys.Team,
                "Pages",
                "Ekip",
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["tr"] = "ekip",
                    ["en"] = "team",
                    ["de"] = "team",
                }),
        };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalizedRouteService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string NormalizeCulture(string? culture) => LocaleCatalog.Normalize(culture);

    public string GetCurrentCulture(HttpContext? httpContext = null)
    {
        var context = httpContext ?? _httpContextAccessor.HttpContext;
        var routeCulture = context?.GetRouteValue(CultureRouteKey)?.ToString();
        if (!string.IsNullOrWhiteSpace(routeCulture))
        {
            return NormalizeCulture(routeCulture);
        }

        return NormalizeCulture(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
    }

    public string Page(string pageKey, string? culture = null, object? queryValues = null)
    {
        var normalizedCulture = NormalizeCulture(culture ?? GetCurrentCulture());
        var page = Pages[pageKey];
        var path = BuildPath(page.GetSlug(normalizedCulture), normalizedCulture);
        return AppendQuery(path, queryValues);
    }

    public string NewsArticle(string slug, string? culture = null)
    {
        var normalizedCulture = NormalizeCulture(culture ?? GetCurrentCulture());
        var listSlug = Pages[SitePageKeys.News].GetSlug(normalizedCulture);
        var articleSlug = slug.Trim().Trim('/');
        return $"/{normalizedCulture}/{listSlug}/{articleSlug}";
    }

    public string CurrentPageInCulture(string targetCulture, HttpContext? httpContext = null)
    {
        var context = httpContext ?? _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return Page(SitePageKeys.Home, targetCulture);
        }

        var pageKey = GetCurrentPageKey(context);
        return pageKey is null
            ? Page(SitePageKeys.Home, targetCulture)
            : Page(pageKey, targetCulture, context.Request.Query);
    }

    public string LocalizeLocalUrl(string? localUrl, string targetCulture)
    {
        if (string.IsNullOrWhiteSpace(localUrl))
        {
            return Page(SitePageKeys.Home, targetCulture);
        }

        var queryIndex = localUrl.IndexOf('?', StringComparison.Ordinal);
        var path = queryIndex >= 0 ? localUrl[..queryIndex] : localUrl;
        var query = queryIndex >= 0 ? QueryHelpers.ParseQuery(localUrl[queryIndex..]) : new Dictionary<string, StringValues>();

        return TryResolvePath(path, out var page, out _)
            ? Page(page.PageKey, targetCulture, query)
            : Page(SitePageKeys.Home, targetCulture);
    }

    public string? GetCurrentPageKey(HttpContext? httpContext = null)
    {
        var context = httpContext ?? _httpContextAccessor.HttpContext;
        var routePageKey = context?.GetRouteValue(PageKeyRouteKey)?.ToString();
        if (!string.IsNullOrWhiteSpace(routePageKey))
        {
            return routePageKey;
        }

        return context is not null && TryResolvePath(context.Request.Path.Value, out var page, out _)
            ? page.PageKey
            : null;
    }

    public bool IsCurrentPage(string pageKey, HttpContext? httpContext = null) =>
        string.Equals(GetCurrentPageKey(httpContext), pageKey, StringComparison.OrdinalIgnoreCase);

    public bool IsCurrentPage(HttpContext? httpContext = null, params string[] pageKeys)
    {
        var current = GetCurrentPageKey(httpContext);
        return current is not null && pageKeys.Contains(current, StringComparer.OrdinalIgnoreCase);
    }

    public string AbsolutePage(HttpContext httpContext, string pageKey, string? culture = null, object? queryValues = null)
        => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{Page(pageKey, culture, queryValues)}";

    public IReadOnlyList<(string Culture, string Url)> GetAlternateAbsoluteUrls(HttpContext httpContext)
    {
        var pageKey = GetCurrentPageKey(httpContext);
        if (pageKey is null)
        {
            return [];
        }

        return SupportedCultures
            .Select(culture => (culture, AbsolutePage(httpContext, pageKey, culture, httpContext.Request.Query)))
            .ToArray();
    }

    public bool TryResolveLocalized(string? culture, string? slug, out LocalizedPageDefinition page)
    {
        var normalizedCulture = NormalizeCulture(culture);
        var normalizedSlug = NormalizeSlug(slug);

        page = Pages.Values.FirstOrDefault(candidate => candidate.HasSlug(normalizedCulture, normalizedSlug))
            ?? null!;

        return page is not null;
    }

    public bool TryResolveLegacyPath(PathString path, out LocalizedPageDefinition page)
    {
        var normalizedPath = NormalizeSlug(path.Value);
        page = Pages.Values.FirstOrDefault(candidate => candidate.HasSlug("tr", normalizedPath))
            ?? null!;

        return page is not null;
    }

    public bool TryResolvePath(string? path, out LocalizedPageDefinition page, out string culture)
    {
        page = null!;
        culture = "tr";
        var normalizedPath = NormalizeSlug(path);
        if (string.IsNullOrEmpty(normalizedPath))
        {
            page = Pages[SitePageKeys.Home];
            return true;
        }

        var slashIndex = normalizedPath.IndexOf('/', StringComparison.Ordinal);
        var firstSegment = slashIndex >= 0 ? normalizedPath[..slashIndex] : normalizedPath;

        if (SupportedCultures.Contains(firstSegment, StringComparer.OrdinalIgnoreCase))
        {
            culture = NormalizeCulture(firstSegment);
            var slug = slashIndex >= 0 ? normalizedPath[(slashIndex + 1)..] : string.Empty;
            return TryResolveLocalized(culture, slug, out page);
        }

        if (TryResolveLegacyPath(new PathString("/" + normalizedPath), out page))
        {
            culture = "tr";
            return true;
        }

        return false;
    }

    private static string BuildPath(string slug, string culture) =>
        string.IsNullOrEmpty(slug) ? $"/{culture}" : $"/{culture}/{slug}";

    private static string NormalizeSlug(string? slug) =>
        (slug ?? string.Empty).Trim().Trim('/').ToLowerInvariant();

    private static string AppendQuery(string path, object? queryValues)
    {
        var values = ToQueryDictionary(queryValues);
        return values.Count == 0 ? path : QueryHelpers.AddQueryString(path, values);
    }

    private static Dictionary<string, string?> ToQueryDictionary(object? queryValues)
    {
        if (queryValues is null)
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        if (queryValues is IQueryCollection queryCollection)
        {
            return queryCollection
                .Where(item => !StringValues.IsNullOrEmpty(item.Value))
                .ToDictionary(
                    item => item.Key,
                    item => (string?)item.Value.ToString(),
                    StringComparer.OrdinalIgnoreCase);
        }

        if (queryValues is IEnumerable<KeyValuePair<string, StringValues>> pairs)
        {
            return pairs
                .Where(item => !StringValues.IsNullOrEmpty(item.Value))
                .ToDictionary(
                    item => item.Key,
                    item => (string?)item.Value.ToString(),
                    StringComparer.OrdinalIgnoreCase);
        }

        var routeValues = new RouteValueDictionary(queryValues);
        return routeValues
            .Where(item => item.Value is not null)
            .ToDictionary(
                item => item.Key,
                item => Convert.ToString(item.Value, CultureInfo.InvariantCulture),
                StringComparer.OrdinalIgnoreCase);
    }
}
