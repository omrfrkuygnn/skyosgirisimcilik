using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using SkyOS.Backoffice.Extensions;
using SkyOS.Backoffice.Localization;

namespace SkyOS.Backoffice.Controllers;

[AllowAnonymous]
public sealed class CultureController : Controller
{
    [HttpGet("/Culture/Set/{culture}")]
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

        return User.Identity?.IsAuthenticated == true
            ? LocalRedirect("/Dashboard")
            : LocalRedirect("/Account/Login");
    }
}
