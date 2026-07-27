using Microsoft.AspNetCore.Mvc.ModelBinding;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Helpers;

public static class ValidationMessages
{
    public static string Summarize(ModelStateDictionary modelState, IAppLocalizer localizer)
    {
        var messages = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error => Localize(entry.Key, error.ErrorMessage, localizer)))
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct()
            .ToList();

        return messages.Count == 0
            ? localizer["Admin.Validation.FormInvalid"]
            : string.Join(" ", messages);
    }

    private static string Localize(string key, string message, IAppLocalizer localizer)
    {
        if (key.EndsWith(nameof(Application.DTOs.Admin.AdminReplyDto.Subject), StringComparison.Ordinal))
        {
            if (message.Contains("empty", StringComparison.OrdinalIgnoreCase) || message.Contains("not empty", StringComparison.OrdinalIgnoreCase))
            {
                return localizer["Admin.Validation.SubjectRequired"];
            }

            if (message.Contains("200", StringComparison.Ordinal))
            {
                return localizer["Admin.Validation.SubjectMax"];
            }
        }

        if (key.EndsWith(nameof(Application.DTOs.Admin.AdminReplyDto.Message), StringComparison.Ordinal))
        {
            if (message.Contains("empty", StringComparison.OrdinalIgnoreCase) || message.Contains("not empty", StringComparison.OrdinalIgnoreCase))
            {
                return localizer["Admin.Validation.MessageRequired"];
            }

            if (message.Contains("10", StringComparison.Ordinal))
            {
                return localizer["Admin.Validation.MessageMin"];
            }

            if (message.Contains("4000", StringComparison.Ordinal))
            {
                return localizer["Admin.Validation.MessageMax"];
            }
        }

        return message;
    }
}
