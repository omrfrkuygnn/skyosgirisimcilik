using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SkyOS.Web.Extensions;
using SkyOS.Web.Localization;
using SkyOS.Web.Routing;

namespace SkyOS.Web.Controllers;

public sealed class CultureController : Controller
{
    private readonly ILocalizedRouteService _routes;

    public CultureController(ILocalizedRouteService routes) => _routes = routes;

    [HttpGet("/dil/{culture}")]
    [HttpGet("/lang/{culture}")]
    public IActionResult Set(string culture, string? returnUrl = null)
    {
        culture = LocaleCatalog.Normalize(culture);
        var value = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
        Response.Cookies.Append(
            LocalizationServiceExtensions.CookieName,
            value,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                Path = "/",
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(_routes.LocalizeLocalUrl(returnUrl, culture));
        }

        return LocalRedirect(_routes.Page(SitePageKeys.Home, culture));
    }
}
