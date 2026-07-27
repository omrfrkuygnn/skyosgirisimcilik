using SkyOS.Domain.Enums;

namespace SkyOS.Application.DTOs.Admin;

public sealed class ContactMessageListItemDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public InterestType InterestType { get; set; }

    public string MessagePreview { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
