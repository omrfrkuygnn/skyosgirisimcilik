using SkyOS.Application.DTOs.Team;

namespace SkyOS.Application.Interfaces.Services;

public interface ITeamService
{
    Task<IReadOnlyList<TeamMemberDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TeamMemberDto>> GetLeadersAsync(CancellationToken cancellationToken = default);
}
