using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace SkyOS.Web.Routing;

public sealed class LocalizedRouteTransformer : DynamicRouteValueTransformer
{
    private readonly ILocalizedRouteService _routes;

    public LocalizedRouteTransformer(ILocalizedRouteService routes) => _routes = routes;

    public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
    {
        var culture = values[LocalizedRouteService.CultureRouteKey]?.ToString();
        var slug = values["slug"]?.ToString();

        if (!_routes.TryResolveLocalized(culture, slug, out var page))
        {
            return ValueTask.FromResult<RouteValueDictionary>(null!);
        }

        return ValueTask.FromResult(new RouteValueDictionary
        {
            ["controller"] = page.Controller,
            ["action"] = page.Action,
            [LocalizedRouteService.CultureRouteKey] = _routes.NormalizeCulture(culture),
            [LocalizedRouteService.PageKeyRouteKey] = page.PageKey,
        });
    }
}
