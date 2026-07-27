using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Shared.Constants;

namespace SkyOS.Backoffice.Controllers;

public sealed class AuditLogsController : AdminControllerBase
{
    public AuditLogsController(IAuditLogService auditLogs) : base(auditLogs) { }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await AuditLogs.ListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.ViewAuditLogs).ConfigureAwait(false);
        return View(items);
    }
}
