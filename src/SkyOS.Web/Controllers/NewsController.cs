using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.Interfaces.Services;

namespace SkyOS.Web.Controllers;

public sealed class NewsController : Controller
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService) => _newsService = newsService;

    [HttpGet("{culture:regex(^tr|en|de$)}/haberler")]
    [HttpGet("{culture:regex(^tr|en|de$)}/news")]
    [HttpGet("{culture:regex(^tr|en|de$)}/nachrichten")]
    public async Task<IActionResult> Index(string culture, CancellationToken cancellationToken)
    {
        var items = await _newsService.GetPublishedAsync(cancellationToken).ConfigureAwait(false);
        return View(items);
    }

    [HttpGet("{culture:regex(^tr|en|de$)}/haberler/{slug}")]
    [HttpGet("{culture:regex(^tr|en|de$)}/news/{slug}")]
    [HttpGet("{culture:regex(^tr|en|de$)}/nachrichten/{slug}")]
    public async Task<IActionResult> Detail(string culture, string slug, CancellationToken cancellationToken)
    {
        var result = await _newsService.GetPublishedBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound();
        }

        return View(result.Value);
    }
}
