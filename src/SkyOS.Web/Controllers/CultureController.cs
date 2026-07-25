using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SkyOS.Web.Extensions;
using SkyOS.Web.Localization;

namespace SkyOS.Web.Controllers;

public sealed class CultureController : Controller
{
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
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect("~/");
    }
}
