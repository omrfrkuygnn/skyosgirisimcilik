using SkyOS.Application.DTOs.Milestones;
using SkyOS.Domain.Enums;

namespace SkyOS.Web.ViewModels;

public sealed class AchievementsPageViewModel
{
    public required IReadOnlyDictionary<MilestoneCategory, IReadOnlyList<MilestoneDto>> MilestonesByCategory { get; init; }

    /// <summary>Market-size projection cited in the source document (Section 7.5).</summary>
    public string MarketProjection =>
        "Otonom sistem yazılımı pazarının 2030 yılına kadar 110 milyar dolara ulaşması öngörülmektedir.";
}
