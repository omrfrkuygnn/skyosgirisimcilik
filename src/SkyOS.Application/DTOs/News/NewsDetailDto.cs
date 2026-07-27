namespace SkyOS.Application.DTOs.News;

public sealed class NewsDetailDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public DateTime PublishedAtUtc { get; init; }

    public bool IsPublished { get; init; }
}
