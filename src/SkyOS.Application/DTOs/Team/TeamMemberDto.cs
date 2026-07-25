namespace SkyOS.Application.DTOs.Team;

public sealed class TeamMemberDto
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string? Bio { get; init; }

    public string? PhotoUrl { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsLeader { get; init; }
}
