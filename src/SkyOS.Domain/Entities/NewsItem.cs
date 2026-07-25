using SkyOS.Domain.Common;

namespace SkyOS.Domain.Entities;

/// <summary>
/// Optional press-release / blog entry. Modelled now so future content requires no schema churn.
/// </summary>
public class NewsItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime PublishedAtUtc { get; set; }

    public bool IsPublished { get; set; }
}
