using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Shared.Constants;

namespace SkyOS.Backoffice.Controllers;

public sealed class MilestonesController : AdminControllerBase
{
    private readonly IContentAdminService _content;

    public MilestonesController(IContentAdminService content, IAuditLogService auditLogs)
        : base(auditLogs) => _content = content;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _content.ListMilestonesAsync(cancellationToken).ConfigureAwait(false);
        return View(items);
    }

    public IActionResult Create() => View(new MilestoneUpsertDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MilestoneUpsertDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.CreateMilestoneAsync(dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.CreateMilestone, "Milestone", result.Value.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _content.GetMilestoneAsync(id, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var milestone = result.Value;
        return View(new MilestoneUpsertDto
        {
            Title = milestone.Title,
            Description = milestone.Description,
            DateAchieved = milestone.DateAchieved,
            Category = milestone.Category,
            DisplayOrder = milestone.DisplayOrder,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MilestoneUpsertDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.UpdateMilestoneAsync(id, dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.UpdateMilestone, "Milestone", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _content.DeleteMilestoneAsync(id, cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.DeleteMilestone, "Milestone", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }
}
