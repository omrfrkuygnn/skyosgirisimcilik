using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Shared.Constants;

namespace SkyOS.Backoffice.Controllers;

public sealed class DashboardController : AdminControllerBase
{
    private readonly IDashboardAdminService _dashboard;

    public DashboardController(IDashboardAdminService dashboard, IAuditLogService auditLogs)
        : base(auditLogs) => _dashboard = dashboard;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var stats = await _dashboard.GetStatsAsync(cancellationToken).ConfigureAwait(false);
        await LogActionAsync(AuditActions.ViewDashboard);
        return View(stats);
    }
}
