using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.DTOs.Milestones;
using SkyOS.Application.DTOs.News;
using SkyOS.Application.DTOs.Partners;
using SkyOS.Application.DTOs.Team;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Services;

public interface IContentAdminService
{
    Task<IReadOnlyList<TeamMemberDto>> ListTeamAsync(CancellationToken cancellationToken = default);
    Task<Result<TeamMemberDto>> GetTeamAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateTeamAsync(TeamMemberUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateTeamAsync(int id, TeamMemberUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteTeamAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerDto>> ListPartnersAsync(CancellationToken cancellationToken = default);
    Task<Result<PartnerDto>> GetPartnerAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> CreatePartnerAsync(PartnerUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdatePartnerAsync(int id, PartnerUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeletePartnerAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MilestoneDto>> ListMilestonesAsync(CancellationToken cancellationToken = default);
    Task<Result<MilestoneDto>> GetMilestoneAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateMilestoneAsync(MilestoneUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateMilestoneAsync(int id, MilestoneUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteMilestoneAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NewsListItemDto>> ListNewsAsync(CancellationToken cancellationToken = default);
    Task<Result<NewsDetailDto>> GetNewsAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<int>> CreateNewsAsync(NewsItemUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateNewsAsync(int id, NewsItemUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteNewsAsync(int id, CancellationToken cancellationToken = default);
}
