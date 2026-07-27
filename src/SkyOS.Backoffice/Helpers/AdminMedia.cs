using Microsoft.Extensions.Options;
using SkyOS.Backoffice.Options;

namespace SkyOS.Backoffice.Helpers;

public static class AdminMedia
{
    public static string? PreviewUrl(string? path, IOptions<ContentUploadOptions> options)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        var baseUrl = options.Value.PublicSiteBaseUrl?.TrimEnd('/') ?? string.Empty;
        return string.IsNullOrEmpty(baseUrl) ? path : baseUrl + (path.StartsWith('/') ? path : "/" + path);
    }
}
