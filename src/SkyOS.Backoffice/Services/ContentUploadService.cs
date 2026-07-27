using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SkyOS.Backoffice.Options;
using SkyOS.Shared.Results;

namespace SkyOS.Backoffice.Services;

public sealed class ContentUploadService : IContentUploadService
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif",
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".html", ".htm",
    };

    private readonly string _webRoot;
    private readonly ContentUploadOptions _options;

    public ContentUploadService(IWebHostEnvironment environment, IOptions<ContentUploadOptions> options)
    {
        _options = options.Value;
        _webRoot = Path.GetFullPath(Path.Combine(environment.ContentRootPath, _options.WebWwwrootPath));
    }

    public Task<Result<string>> SaveImageAsync(
        IFormFile file,
        string folder,
        bool compress = true,
        CancellationToken cancellationToken = default) =>
        SaveImageInternalAsync(file, folder, compress, cancellationToken);

    public Task<Result<string>> SaveDocumentAsync(IFormFile file, string folder, CancellationToken cancellationToken = default) =>
        SaveAsync(file, folder, DocumentExtensions, _options.MaxDocumentBytes, cancellationToken);

    private async Task<Result<string>> SaveImageInternalAsync(
        IFormFile file,
        string folder,
        bool compress,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return Result.Failure<string>(Error.Validation("Empty file."));
        }

        if (file.Length > _options.MaxImageBytes)
        {
            return Result.Failure<string>(Error.Validation("File is too large."));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!ImageExtensions.Contains(extension))
        {
            return Result.Failure<string>(Error.Validation("File type is not allowed."));
        }

        var safeFolder = SanitizeFolder(folder);
        var uploadDir = Path.Combine(_webRoot, "uploads", safeFolder);
        Directory.CreateDirectory(uploadDir);

        // Animated GIF: keep original unless user asked to compress (then still keep to preserve animation).
        if (string.Equals(extension, ".gif", StringComparison.OrdinalIgnoreCase))
        {
            return await SaveRawAsync(file, safeFolder, uploadDir, extension, cancellationToken).ConfigureAwait(false);
        }

        if (!compress)
        {
            return await SaveRawAsync(file, safeFolder, uploadDir, extension, cancellationToken).ConfigureAwait(false);
        }

        return await SaveOptimizedWebpAsync(file, safeFolder, uploadDir, cancellationToken).ConfigureAwait(false);
    }

    private async Task<Result<string>> SaveOptimizedWebpAsync(
        IFormFile file,
        string safeFolder,
        string uploadDir,
        CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid():N}.webp";
        var physicalPath = Path.Combine(uploadDir, fileName);

        await using var inputStream = file.OpenReadStream();
        using var image = await Image.LoadAsync(inputStream, cancellationToken).ConfigureAwait(false);

        var maxDimension = _options.MaxImageDimension;
        if (image.Width > maxDimension || image.Height > maxDimension)
        {
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(maxDimension, maxDimension),
                Mode = ResizeMode.Max,
            }));
        }

        var quality = file.Length > _options.ImageWarningBytes
            ? _options.WebpQualityLarge
            : _options.WebpQualityNormal;

        var encoder = new WebpEncoder
        {
            Quality = quality,
            FileFormat = WebpFileFormatType.Lossy,
        };

        await image.SaveAsync(physicalPath, encoder, cancellationToken).ConfigureAwait(false);

        return Result.Success($"/uploads/{safeFolder}/{fileName}");
    }

    private static async Task<Result<string>> SaveRawAsync(
        IFormFile file,
        string safeFolder,
        string uploadDir,
        string extension,
        CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var physicalPath = Path.Combine(uploadDir, fileName);

        await using (var stream = File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
        }

        return Result.Success($"/uploads/{safeFolder}/{fileName}");
    }

    private async Task<Result<string>> SaveAsync(
        IFormFile file,
        string folder,
        HashSet<string> allowedExtensions,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return Result.Failure<string>(Error.Validation("Empty file."));
        }

        if (file.Length > maxBytes)
        {
            return Result.Failure<string>(Error.Validation("File is too large."));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(extension))
        {
            return Result.Failure<string>(Error.Validation("File type is not allowed."));
        }

        var safeFolder = SanitizeFolder(folder);
        var uploadDir = Path.Combine(_webRoot, "uploads", safeFolder);
        Directory.CreateDirectory(uploadDir);

        return await SaveRawAsync(file, safeFolder, uploadDir, extension, cancellationToken).ConfigureAwait(false);
    }

    private static string SanitizeFolder(string folder)
    {
        var parts = folder.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join('/', parts.Select(p => new string(p.Where(char.IsLetterOrDigit).ToArray())).Where(p => p.Length > 0));
    }
}
