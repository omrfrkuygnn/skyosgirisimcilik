using SkyOS.Application.DTOs.News;
using SkyOS.Shared.Results;

namespace SkyOS.Application.Interfaces.Services;

public interface INewsService
{
    Task<IReadOnlyList<NewsListItemDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<Result<NewsDetailDto>> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
