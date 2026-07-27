namespace SkyOS.Web.Routing;

public sealed class LocalizedPageDefinition
{
    private readonly IReadOnlyDictionary<string, string> _slugs;

    public LocalizedPageDefinition(
        string pageKey,
        string controller,
        string action,
        IReadOnlyDictionary<string, string> slugs)
    {
        PageKey = pageKey;
        Controller = controller;
        Action = action;
        _slugs = slugs;
    }

    public string PageKey { get; }

    public string Controller { get; }

    public string Action { get; }

    public string GetSlug(string culture) => _slugs[culture];

    public bool HasSlug(string culture, string slug) =>
        string.Equals(GetSlug(culture), slug, StringComparison.OrdinalIgnoreCase);

    public string TurkishSlug => GetSlug("tr");
}
