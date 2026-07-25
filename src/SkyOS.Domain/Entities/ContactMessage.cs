using SkyOS.Domain.Common;
using SkyOS.Domain.Enums;

namespace SkyOS.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public InterestType InterestType { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    /// <summary>Origin IP, retained for abuse investigation only. Treated as PII in logs.</summary>
    public string? IpAddress { get; set; }
}
