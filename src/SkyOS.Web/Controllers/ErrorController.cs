using Microsoft.AspNetCore.Mvc;

namespace SkyOS.Web.Controllers;

/// <summary>
/// Renders branded error pages. Reached via UseStatusCodePagesWithReExecute ("/hata/{0}")
/// for status codes and UseExceptionHandler ("/hata/500") for unhandled exceptions.
/// Production never leaks stack traces.
/// </summary>
[Route("hata")]
public sealed class ErrorController : Controller
{
    [HttpGet("{code:int}")]
    public IActionResult Status(int code)
    {
        Response.StatusCode = code;
        return code == 404 ? View("NotFound") : View("ServerError");
    }
}
