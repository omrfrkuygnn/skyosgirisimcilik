using Microsoft.AspNetCore.Mvc.ModelBinding;
using SkyOS.Backoffice.Services;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Helpers;

public static class ContentFormFiles
{
    public static async Task<bool> TryApplyImageAsync(
        ModelStateDictionary modelState,
        IContentUploadService uploads,
        IAppLocalizer localizer,
        string folder,
        IFormFile? file,
        Action<string> setUrl,
        bool compress = true,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return true;
        }

        var result = await uploads.SaveImageAsync(file, folder, compress, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            modelState.AddModelError(string.Empty, UploadErrorMessage(localizer, result.Error));
            return false;
        }

        setUrl(result.Value);
        return true;
    }

    public static async Task<bool> TryApplyDocumentAsync(
        ModelStateDictionary modelState,
        IContentUploadService uploads,
        IAppLocalizer localizer,
        string folder,
        IFormFile? file,
        Action<string> setUrl,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return true;
        }

        var result = await uploads.SaveDocumentAsync(file, folder, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            modelState.AddModelError(string.Empty, UploadErrorMessage(localizer, result.Error));
            return false;
        }

        setUrl(result.Value);
        return true;
    }

    private static string UploadErrorMessage(IAppLocalizer localizer, Shared.Results.Error? error) =>
        error?.Message switch
        {
            "File is too large." => localizer["Admin.Error.FileTooLarge"],
            "File type is not allowed." => localizer["Admin.Error.InvalidFileType"],
            _ => localizer["Admin.Error.UploadFailed"],
        };
}
