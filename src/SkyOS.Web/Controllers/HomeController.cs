using Microsoft.AspNetCore.Mvc;

namespace SkyOS.Web.Controllers;

public sealed class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
