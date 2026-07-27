using SkyOS.Shared.Results;

namespace SkyOS.Backoffice.Services;

public interface IContentUploadService
{
    /// <param name="compress">When true, convert/optimize to WebP. When false, store original bytes.</param>
    Task<Result<string>> SaveImageAsync(
        IFormFile file,
        string folder,
        bool compress = true,
        CancellationToken cancellationToken = default);

    Task<Result<string>> SaveDocumentAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
}
