using Mapster;
using SkyOS.Application.DTOs.News;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Domain.Entities;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Services;

public sealed class NewsService : INewsService
{
    private readonly IUnitOfWork _unitOfWork;

    public NewsService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<NewsListItemDto>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.Repository<NewsItem>()
            .ListAsync(x => x.IsPublished, cancellationToken)
            .ConfigureAwait(false);

        return items
            .OrderByDescending(x => x.PublishedAtUtc)
            .Select(x => x.Adapt<NewsListItemDto>())
            .ToList();
    }

    public async Task<Result<NewsDetailDto>> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeSlug(slug);
        var items = await _unitOfWork.Repository<NewsItem>()
            .ListAsync(x => x.IsPublished && x.Slug == normalized, cancellationToken)
            .ConfigureAwait(false);

        var entity = items.FirstOrDefault();
        return entity is null
            ? Result.Failure<NewsDetailDto>(Error.NotFound("News item not found."))
            : Result.Success(entity.Adapt<NewsDetailDto>());
    }

    private static string NormalizeSlug(string slug) =>
        slug.Trim().Trim('/').ToLowerInvariant();
}
