namespace SkyOS.Application.DTOs.Contact;

public sealed class ContactMessageResponseDto
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }
}
