using Microsoft.AspNetCore.Localization;

namespace SkyOS.Backoffice.Localization;

public sealed class RouteValueRequestCultureProvider : RequestCultureProvider
{
    public string RouteValueKey { get; init; } = "culture";

    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!httpContext.Request.RouteValues.TryGetValue(RouteValueKey, out var value) || value is null)
        {
            return NullProviderCultureResult;
        }

        var culture = LocaleCatalog.Normalize(value.ToString());
        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture, culture));
    }
}
