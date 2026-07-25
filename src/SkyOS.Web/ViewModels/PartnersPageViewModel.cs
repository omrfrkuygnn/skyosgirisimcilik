using SkyOS.Application.DTOs.Partners;

namespace SkyOS.Web.ViewModels;

public sealed class PartnersPageViewModel
{
    public required IReadOnlyList<PartnerDto> VerifiedPartners { get; init; }

    public required IReadOnlyList<PartnerDto> OtherPartners { get; init; }
}
