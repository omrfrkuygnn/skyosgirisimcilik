using Mapster;
using SkyOS.Application.DTOs.Team;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;

namespace SkyOS.Application.Services;

public sealed class TeamService : ITeamService
{
    private readonly IUnitOfWork _unitOfWork;

    public TeamService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<TeamMemberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var members = await _unitOfWork.Repository<TeamMember>().ListAsync(cancellationToken).ConfigureAwait(false);
        return members
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.FullName)
            .Select(m => m.Adapt<TeamMemberDto>())
            .ToList();
    }

    public async Task<IReadOnlyList<TeamMemberDto>> GetLeadersAsync(CancellationToken cancellationToken = default)
    {
        var leaders = await _unitOfWork.Repository<TeamMember>()
            .ListAsync(m => m.IsLeader, cancellationToken)
            .ConfigureAwait(false);

        return leaders
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.FullName)
            .Select(m => m.Adapt<TeamMemberDto>())
            .ToList();
    }
}
