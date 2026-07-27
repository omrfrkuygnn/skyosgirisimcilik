using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Shared.Constants;

namespace SkyOS.Backoffice.Controllers;

public sealed class NewsItemsController : AdminControllerBase
{
    private readonly IContentAdminService _content;

    public NewsItemsController(IContentAdminService content, IAuditLogService auditLogs)
        : base(auditLogs) => _content = content;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _content.ListNewsAsync(cancellationToken).ConfigureAwait(false);
        return View(items);
    }

    public IActionResult Create() => View(new NewsItemUpsertDto { PublishedAtUtc = DateTime.UtcNow, IsPublished = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewsItemUpsertDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.CreateNewsAsync(dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.CreateNewsItem, "NewsItem", result.Value.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _content.GetNewsAsync(id, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var news = result.Value;
        return View(new NewsItemUpsertDto
        {
            Title = news.Title,
            Slug = news.Slug,
            Summary = news.Summary,
            Body = news.Body,
            PublishedAtUtc = news.PublishedAtUtc,
            IsPublished = news.IsPublished,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NewsItemUpsertDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.UpdateNewsAsync(id, dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.UpdateNewsItem, "NewsItem", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _content.DeleteNewsAsync(id, cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.DeleteNewsItem, "NewsItem", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }
}
