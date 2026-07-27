using SkyOS.Domain.Enums;

namespace SkyOS.Application.DTOs.Admin;

public sealed class ContactMessageDetailDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public InterestType InterestType { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
