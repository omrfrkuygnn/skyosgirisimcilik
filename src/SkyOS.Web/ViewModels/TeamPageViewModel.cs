using SkyOS.Application.DTOs.Team;

namespace SkyOS.Web.ViewModels;

public sealed class TeamPageViewModel
{
    public required IReadOnlyList<TeamMemberDto> Leaders { get; init; }

    public required IReadOnlyList<TeamMemberDto> Members { get; init; }

    public int TargetHeadcountLow => 40;

    public int TargetHeadcountHigh => 60;

    public int CurrentHeadcount => 15;
}
