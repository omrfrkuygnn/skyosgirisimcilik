using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Backoffice.Helpers;
using SkyOS.Shared.Constants;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Controllers;

public sealed class SiteFeedbacksController : AdminControllerBase
{
    private readonly ISiteFeedbackAdminService _service;
    private readonly IAdminReplyService _replyService;
    private readonly IAppLocalizer _localizer;

    public SiteFeedbacksController(
        ISiteFeedbackAdminService service,
        IAdminReplyService replyService,
        IAuditLogService auditLogs,
        IAppLocalizer localizer)
        : base(auditLogs)
    {
        _service = service;
        _replyService = replyService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _service.ListAsync(cancellationToken).ConfigureAwait(false);
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound();
        }

        await _service.MarkAsReadAsync(id, cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.ViewSiteFeedback, "SiteFeedback", id.ToString()).ConfigureAwait(false);
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, [Bind(Prefix = "")] AdminReplyDto reply, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ReplyError"] = ValidationMessages.Summarize(ModelState, _localizer);
            return RedirectToAction(nameof(Details), new { id });
        }

        var adminEmail = User.Identity?.Name ?? string.Empty;
        var adminName = User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value
            ?? User.FindFirst("display_name")?.Value;

        var result = await _replyService
            .ReplyToFeedbackAsync(id, reply, adminEmail, adminName, cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            TempData["ReplyError"] = AdminDisplay.ReplyErrorMessage(result.Error, _localizer);
            return RedirectToAction(nameof(Details), new { id });
        }

        await LogActionAsync(
            AuditActions.ReplySiteFeedback,
            "SiteFeedback",
            id.ToString(),
            reply.Subject).ConfigureAwait(false);

        TempData["ReplySuccess"] = true;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        await _service.MarkAsReadAsync(id, cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.MarkFeedbackRead, "SiteFeedback", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.DeleteSiteFeedback, "SiteFeedback", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }
}
