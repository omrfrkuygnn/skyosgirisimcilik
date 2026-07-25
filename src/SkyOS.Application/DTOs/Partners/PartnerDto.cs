namespace SkyOS.Application.DTOs.Partners;

public sealed class PartnerDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }

    public string? Description { get; init; }

    public string? Address { get; init; }

    public string? Phone { get; init; }

    public string? WebsiteUrl { get; init; }

    public string? SupportLetterUrl { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsVerifiedRelationship { get; init; }
}
