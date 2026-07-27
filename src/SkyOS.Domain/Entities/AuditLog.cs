using SkyOS.Domain.Common;

namespace SkyOS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }
}
