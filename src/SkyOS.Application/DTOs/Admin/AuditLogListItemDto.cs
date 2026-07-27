namespace SkyOS.Application.DTOs.Admin;

public sealed class AuditLogListItemDto
{
    public int Id { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
