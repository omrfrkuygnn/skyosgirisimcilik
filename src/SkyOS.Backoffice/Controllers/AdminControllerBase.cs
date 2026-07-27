using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Shared.Constants;

namespace SkyOS.Backoffice.Controllers;

public abstract class AdminControllerBase : Controller
{
    protected readonly IAuditLogService AuditLogs;

    protected AdminControllerBase(IAuditLogService auditLogs) => AuditLogs = auditLogs;

    protected Task LogActionAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        string? details = null)
    {
        return AuditLogs.WriteAsync(new AuditLogWriteDto
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            UserEmail = User.Identity?.Name ?? string.Empty,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
        });
    }
}
