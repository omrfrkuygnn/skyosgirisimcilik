using Mapster;
using SkyOS.Application.DTOs.Admin;
using SkyOS.Application.DTOs.Milestones;
using SkyOS.Application.DTOs.News;
using SkyOS.Application.DTOs.Partners;
using SkyOS.Application.DTOs.Team;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Services;

public sealed class ContentAdminService : IContentAdminService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContentAdminService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<TeamMemberDto>> ListTeamAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<TeamMember>().ListAsync(cancellationToken).ConfigureAwait(false);
        return items.OrderBy(x => x.DisplayOrder).ThenBy(x => x.FullName).Select(x => x.Adapt<TeamMemberDto>()).ToList();
    }

    public async Task<Result<TeamMemberDto>> GetTeamAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<TeamMember>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null
            ? Result.Failure<TeamMemberDto>(Error.NotFound("Team member not found."))
            : Result.Success(entity.Adapt<TeamMemberDto>());
    }

    public async Task<Result<int>> CreateTeamAsync(TeamMemberUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = dto.Adapt<TeamMember>();
        await _unitOfWork.Repository<TeamMember>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(entity.Id);
    }

    public async Task<Result> UpdateTeamAsync(int id, TeamMemberUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<TeamMember>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("Team member not found."));
        dto.Adapt(entity);
        _unitOfWork.Repository<TeamMember>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteTeamAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<TeamMember>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("Team member not found."));
        _unitOfWork.Repository<TeamMember>().Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<IReadOnlyList<PartnerDto>> ListPartnersAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Partner>().ListAsync(cancellationToken).ConfigureAwait(false);
        return items.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).Select(x => x.Adapt<PartnerDto>()).ToList();
    }

    public async Task<Result<PartnerDto>> GetPartnerAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Partner>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null
            ? Result.Failure<PartnerDto>(Error.NotFound("Partner not found."))
            : Result.Success(entity.Adapt<PartnerDto>());
    }

    public async Task<Result<int>> CreatePartnerAsync(PartnerUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = dto.Adapt<Partner>();
        await _unitOfWork.Repository<Partner>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(entity.Id);
    }

    public async Task<Result> UpdatePartnerAsync(int id, PartnerUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Partner>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("Partner not found."));
        dto.Adapt(entity);
        _unitOfWork.Repository<Partner>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeletePartnerAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Partner>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("Partner not found."));
        _unitOfWork.Repository<Partner>().Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<IReadOnlyList<MilestoneDto>> ListMilestonesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<Milestone>().ListAsync(cancellationToken).ConfigureAwait(false);
        return items.OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.DateAchieved).Select(x => x.Adapt<MilestoneDto>()).ToList();
    }

    public async Task<Result<MilestoneDto>> GetMilestoneAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Milestone>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null
            ? Result.Failure<MilestoneDto>(Error.NotFound("Milestone not found."))
            : Result.Success(entity.Adapt<MilestoneDto>());
    }

    public async Task<Result<int>> CreateMilestoneAsync(MilestoneUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = dto.Adapt<Milestone>();
        await _unitOfWork.Repository<Milestone>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(entity.Id);
    }

    public async Task<Result> UpdateMilestoneAsync(int id, MilestoneUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Milestone>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("Milestone not found."));
        dto.Adapt(entity);
        _unitOfWork.Repository<Milestone>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteMilestoneAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<Milestone>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("Milestone not found."));
        _unitOfWork.Repository<Milestone>().Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<IReadOnlyList<NewsListItemDto>> ListNewsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<NewsItem>().ListAsync(cancellationToken).ConfigureAwait(false);
        return items.OrderByDescending(x => x.PublishedAtUtc).Select(x => x.Adapt<NewsListItemDto>()).ToList();
    }

    public async Task<Result<NewsDetailDto>> GetNewsAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<NewsItem>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return entity is null
            ? Result.Failure<NewsDetailDto>(Error.NotFound("News item not found."))
            : Result.Success(entity.Adapt<NewsDetailDto>());
    }

    public async Task<Result<int>> CreateNewsAsync(NewsItemUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = dto.Adapt<NewsItem>();
        entity.Slug = NormalizeSlug(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Title : dto.Slug);
        await _unitOfWork.Repository<NewsItem>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(entity.Id);
    }

    public async Task<Result> UpdateNewsAsync(int id, NewsItemUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<NewsItem>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("News item not found."));
        dto.Adapt(entity);
        entity.Slug = NormalizeSlug(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Title : dto.Slug);
        _unitOfWork.Repository<NewsItem>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    public async Task<Result> DeleteNewsAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.Repository<NewsItem>().GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (entity is null) return Result.Failure(Error.NotFound("News item not found."));
        _unitOfWork.Repository<NewsItem>().Remove(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private static string NormalizeSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        var chars = slug.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        slug = new string(chars);
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }
}
