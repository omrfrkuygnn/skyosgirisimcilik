using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Web.Routing;

namespace SkyOS.Web.Controllers;

public sealed class SeoController : Controller
{
    private static readonly string[] Cultures = ["tr", "en", "de"];

    private readonly ILocalizedRouteService _routes;
    private readonly INewsService _newsService;

    public SeoController(ILocalizedRouteService routes, INewsService newsService)
    {
        _routes = routes;
        _newsService = newsService;
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 3600)]
    public ContentResult Robots()
    {
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine($"Sitemap: {Request.Scheme}://{Request.Host}/sitemap.xml");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<ContentResult> Sitemap(CancellationToken cancellationToken)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var staticPages = new[]
        {
            SitePageKeys.Home,
            SitePageKeys.About,
            SitePageKeys.Product,
            SitePageKeys.UseCases,
            SitePageKeys.Achievements,
            SitePageKeys.Investors,
            SitePageKeys.Partners,
            SitePageKeys.Team,
            SitePageKeys.Contact,
            SitePageKeys.Feedback,
            SitePageKeys.News,
            SitePageKeys.PrivacyPolicy,
        };

        var news = await _newsService.GetPublishedAsync(cancellationToken).ConfigureAwait(false);
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset");

        foreach (var culture in Cultures)
        {
            foreach (var pageKey in staticPages)
            {
                urlset.Add(new XElement(ns + "url",
                    new XElement(ns + "loc", baseUrl + _routes.Page(pageKey, culture)),
                    new XElement(ns + "changefreq", "weekly")));
            }

            foreach (var item in news)
            {
                urlset.Add(new XElement(ns + "url",
                    new XElement(ns + "loc", baseUrl + _routes.NewsArticle(item.Slug, culture)),
                    new XElement(ns + "lastmod", item.PublishedAtUtc.ToString("yyyy-MM-dd")),
                    new XElement(ns + "changefreq", "monthly")));
            }
        }

        var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }
}
