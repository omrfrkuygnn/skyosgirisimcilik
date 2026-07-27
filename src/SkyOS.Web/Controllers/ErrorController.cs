using System.Globalization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SkyOS.Web.Localization;

namespace SkyOS.Web.Controllers;

/// <summary>
/// Renders branded error pages. Reached via UseStatusCodePagesWithReExecute ("/hata/{0}")
/// for status codes and UseExceptionHandler ("/hata/500") for unhandled exceptions.
/// Production never leaks stack traces.
/// </summary>
[Route("hata")]
public sealed class ErrorController : Controller
{
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase) { "tr", "en", "de" };

    [HttpGet("{code:int}")]
    public IActionResult Status(int code)
    {
        ApplyCultureFromOriginalPath();
        Response.StatusCode = code;
        return code == 404 ? View("NotFound") : View("ServerError");
    }

    private void ApplyCultureFromOriginalPath()
    {
        var originalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;
        if (string.IsNullOrWhiteSpace(originalPath))
        {
            return;
        }

        var firstSegment = originalPath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (firstSegment is null || !SupportedCultures.Contains(firstSegment))
        {
            return;
        }

        var culture = LocaleCatalog.Normalize(firstSegment);
        var cultureInfo = new CultureInfo(culture);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;
    }
}
