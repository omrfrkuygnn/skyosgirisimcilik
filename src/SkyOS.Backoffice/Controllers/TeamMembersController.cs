using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Backoffice.Helpers;
using SkyOS.Backoffice.Services;
using SkyOS.Shared.Constants;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Controllers;

public sealed class TeamMembersController : AdminControllerBase
{
    private readonly IContentAdminService _content;
    private readonly IContentUploadService _uploads;
    private readonly IAppLocalizer _localizer;

    public TeamMembersController(
        IContentAdminService content,
        IContentUploadService uploads,
        IAuditLogService auditLogs,
        IAppLocalizer localizer)
        : base(auditLogs)
    {
        _content = content;
        _uploads = uploads;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _content.ListTeamAsync(cancellationToken).ConfigureAwait(false);
        return View(items);
    }

    public IActionResult Create() => View(new TeamMemberUpsertDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TeamMemberUpsertDto dto,
        IFormFile? photoFile,
        bool photoCompress = true,
        CancellationToken cancellationToken = default)
    {
        if (!await ContentFormFiles.TryApplyImageAsync(
                ModelState, _uploads, _localizer, "team", photoFile, url => dto.PhotoUrl = url,
                compress: photoCompress, cancellationToken: cancellationToken))
        {
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.CreateTeamAsync(dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.CreateTeamMember, "TeamMember", result.Value.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _content.GetTeamAsync(id, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var member = result.Value;
        return View(new TeamMemberUpsertDto
        {
            FullName = member.FullName,
            Role = member.Role,
            Bio = member.Bio,
            PhotoUrl = member.PhotoUrl,
            DisplayOrder = member.DisplayOrder,
            IsLeader = member.IsLeader,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        TeamMemberUpsertDto dto,
        IFormFile? photoFile,
        bool photoCompress = true,
        CancellationToken cancellationToken = default)
    {
        if (!await ContentFormFiles.TryApplyImageAsync(
                ModelState, _uploads, _localizer, "team", photoFile, url => dto.PhotoUrl = url,
                compress: photoCompress, cancellationToken: cancellationToken))
        {
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.UpdateTeamAsync(id, dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.UpdateTeamMember, "TeamMember", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _content.DeleteTeamAsync(id, cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.DeleteTeamMember, "TeamMember", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }
}
