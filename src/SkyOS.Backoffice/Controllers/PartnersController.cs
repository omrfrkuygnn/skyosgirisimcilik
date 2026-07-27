using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Backoffice.Helpers;
using SkyOS.Backoffice.Services;
using SkyOS.Shared.Constants;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Controllers;

public sealed class PartnersController : AdminControllerBase
{
    private readonly IContentAdminService _content;
    private readonly IContentUploadService _uploads;
    private readonly IAppLocalizer _localizer;

    public PartnersController(
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
        var items = await _content.ListPartnersAsync(cancellationToken).ConfigureAwait(false);
        return View(items);
    }

    public IActionResult Create() => View(new PartnerUpsertDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PartnerUpsertDto dto,
        IFormFile? logoFile,
        IFormFile? supportLetterFile,
        bool logoCompress = true,
        CancellationToken cancellationToken = default)
    {
        if (!await ContentFormFiles.TryApplyImageAsync(
                ModelState, _uploads, _localizer, "partners", logoFile, url => dto.LogoUrl = url,
                compress: logoCompress, cancellationToken: cancellationToken)
            || !await ContentFormFiles.TryApplyDocumentAsync(
                ModelState, _uploads, _localizer, "documents", supportLetterFile, url => dto.SupportLetterUrl = url,
                cancellationToken: cancellationToken))
        {
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.CreatePartnerAsync(dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.CreatePartner, "Partner", result.Value.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await _content.GetPartnerAsync(id, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return NotFound();
        }

        var partner = result.Value;
        return View(new PartnerUpsertDto
        {
            Name = partner.Name,
            LogoUrl = partner.LogoUrl,
            Description = partner.Description,
            Address = partner.Address,
            Phone = partner.Phone,
            WebsiteUrl = partner.WebsiteUrl,
            SupportLetterUrl = partner.SupportLetterUrl,
            DisplayOrder = partner.DisplayOrder,
            IsVerifiedRelationship = partner.IsVerifiedRelationship,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PartnerUpsertDto dto,
        IFormFile? logoFile,
        IFormFile? supportLetterFile,
        bool logoCompress = true,
        CancellationToken cancellationToken = default)
    {
        if (!await ContentFormFiles.TryApplyImageAsync(
                ModelState, _uploads, _localizer, "partners", logoFile, url => dto.LogoUrl = url,
                compress: logoCompress, cancellationToken: cancellationToken)
            || !await ContentFormFiles.TryApplyDocumentAsync(
                ModelState, _uploads, _localizer, "documents", supportLetterFile, url => dto.SupportLetterUrl = url,
                cancellationToken: cancellationToken))
        {
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var result = await _content.UpdatePartnerAsync(id, dto, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Error");
            return View(dto);
        }

        await LogActionAsync(AuditActions.UpdatePartner, "Partner", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _content.DeletePartnerAsync(id, cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.DeletePartner, "Partner", id.ToString()).ConfigureAwait(false);
        return RedirectToAction(nameof(Index));
    }
}
