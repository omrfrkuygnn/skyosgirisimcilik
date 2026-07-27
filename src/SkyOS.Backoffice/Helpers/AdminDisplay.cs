using SkyOS.Domain.Enums;
using SkyOS.Shared.Localization;

namespace SkyOS.Backoffice.Helpers;

public static class AdminDisplay
{
    public static string InterestLabel(this IAppLocalizer localizer, InterestType value) =>
        localizer[$"Enums.Interest.{value}"];

    public static string FeedbackCategoryLabel(this IAppLocalizer localizer, FeedbackCategory value) =>
        localizer[$"Enums.FeedbackCategory.{value}"];

    public static string MilestoneCategoryLabel(this IAppLocalizer localizer, MilestoneCategory value) =>
        localizer[$"Enums.MilestoneCategory.{value}"];

    public static string AuditActionLabel(this IAppLocalizer localizer, string action)
    {
        var key = $"Admin.AuditAction.{action}";
        var label = localizer[key];
        return label == key ? action : label;
    }

    public static string EntityTypeLabel(this IAppLocalizer localizer, string? entityType)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            return "—";
        }

        var key = $"Admin.EntityType.{entityType}";
        var label = localizer[key];
        return label == key ? entityType : label;
    }

    public static string AuditDetailLabel(IAppLocalizer localizer, string? details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return "—";
        }

        return details switch
        {
            "Successful login" => localizer["Admin.AuditDetail.LoginSuccess"],
            "Failed login attempt" => localizer["Admin.AuditDetail.LoginFailed"],
            _ => details,
        };
    }

    public static string CultureUrl(string culture, string returnUrl) =>
        $"/Culture/Set/{culture}?returnUrl={Uri.EscapeDataString(returnUrl)}";

    public static string ReplyErrorMessage(Shared.Results.Error? error, IAppLocalizer localizer)
    {
        if (error is null)
        {
            return localizer["Admin.ReplyFailed"];
        }

        return error.Code switch
        {
            "General.NotFound" => localizer["Admin.Error.NotFound"],
            "General.Validation" when error.Message.Contains("e-posta", StringComparison.OrdinalIgnoreCase)
                || error.Message.Contains("email", StringComparison.OrdinalIgnoreCase)
                => localizer["Admin.Error.NoEmailOnRecord"],
            "General.Failure" => localizer["Admin.ReplyFailed"],
            _ => string.IsNullOrWhiteSpace(error.Message) ? localizer["Admin.ReplyFailed"] : error.Message,
        };
    }
}
