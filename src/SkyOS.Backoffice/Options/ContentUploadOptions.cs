namespace SkyOS.Backoffice.Options;

public sealed class ContentUploadOptions
{
    public const string SectionName = "ContentUpload";

    /// <summary>Relative path from Backoffice content root to SkyOS.Web wwwroot.</summary>
    public string WebWwwrootPath { get; set; } = "../SkyOS.Web/wwwroot";

    public string PublicSiteBaseUrl { get; set; } = "https://localhost:7022";

    /// <summary>Hard ceiling for any image (compressed or original). Soft choice dialog uses ImageWarningBytes.</summary>
    public long MaxImageBytes { get; set; } = 15 * 1024 * 1024;

    /// <summary>Show a warning in the UI when the selected image exceeds this size.</summary>
    public long ImageWarningBytes { get; set; } = 1024 * 1024;

    public int MaxImageDimension { get; set; } = 1600;

    public int WebpQualityNormal { get; set; } = 82;

    public int WebpQualityLarge { get; set; } = 72;

    public long MaxDocumentBytes { get; set; } = 10 * 1024 * 1024;
}
