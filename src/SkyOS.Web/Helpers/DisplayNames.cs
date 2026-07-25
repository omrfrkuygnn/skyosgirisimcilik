using Microsoft.AspNetCore.Mvc.Rendering;
using SkyOS.Domain.Enums;
using SkyOS.Shared.Localization;

namespace SkyOS.Web.Helpers;

/// <summary>
/// Maps domain enums to localized display labels. Kept in the Web layer so the Domain stays
/// free of presentation concerns.
/// </summary>
public static class DisplayNames
{
    public static string Label(this InterestType value, IAppLocalizer L) =>
        L[$"Enums.Interest.{value}"];

    public static string Label(this MilestoneCategory value, IAppLocalizer L) =>
        L[$"Enums.MilestoneCategory.{value}"];

    public static IReadOnlyList<SelectListItem> InterestSelectList(IAppLocalizer L, InterestType? selected = null) =>
        Enum.GetValues<InterestType>()
            .Select(v => new SelectListItem(L[$"Enums.Interest.{v}"], ((int)v).ToString(), selected == v))
            .ToList();

    public static IReadOnlyList<SelectListItem> CountryCodeSelectList(string? selected = "+90")
    {
        var list = new (string Code, string ShortName, string Flag)[]
        {
            ("+90", "TR", "🇹🇷"),
            ("+1", "US/CA", "🇺🇸"),
            ("+44", "UK", "🇬🇧"),
            ("+49", "DE", "🇩🇪"),
            ("+33", "FR", "🇫🇷"),
            ("+31", "NL", "🇳🇱"),
            ("+39", "IT", "🇮🇹"),
            ("+34", "ES", "🇪🇸"),
            ("+41", "CH", "🇨🇭"),
            ("+43", "AT", "🇦🇹"),
            ("+32", "BE", "🇧🇪"),
            ("+46", "SE", "🇸🇪"),
            ("+47", "NO", "🇳🇴"),
            ("+45", "DK", "🇩🇰"),
            ("+358", "FI", "🇫🇮"),
            ("+971", "AE", "🇦🇪"),
            ("+966", "SA", "🇸🇦"),
            ("+974", "QA", "🇶🇦"),
            ("+994", "AZ", "🇦🇿"),
            ("+7", "KZ/RU", "🇰🇿"),
            ("+998", "UZ", "🇺🇿"),
            ("+81", "JP", "🇯🇵"),
            ("+86", "CN", "🇨🇳"),
            ("+91", "IN", "🇮🇳"),
            ("+61", "AU", "🇦🇺"),
            ("+55", "BR", "🇧🇷"),
            ("+82", "KR", "🇰🇷"),
            ("+65", "SG", "🇸🇬"),
            ("+60", "MY", "🇲🇾"),
            ("+20", "EG", "🇪🇬"),
            ("+27", "ZA", "🇿🇦"),
        };

        return list.Select(c => new SelectListItem(
            text: $"{c.Flag} {c.Code} ({c.ShortName})",
            value: c.Code,
            selected: string.Equals(selected, c.Code, StringComparison.OrdinalIgnoreCase)
        )).ToList();
    }
}
