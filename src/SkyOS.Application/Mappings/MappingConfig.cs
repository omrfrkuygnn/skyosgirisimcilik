using Mapster;
using SkyOS.Application.DTOs.Contact;
using SkyOS.Application.DTOs.Milestones;
using SkyOS.Application.DTOs.Partners;
using SkyOS.Application.DTOs.Team;
using SkyOS.Domain.Entities;

namespace SkyOS.Application.Mappings;

/// <summary>
/// Central Mapster configuration. Registered once at startup so entities are never
/// exposed to views and mapping rules stay in a single, testable place.
/// </summary>
public static class MappingConfig
{
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TeamMember, TeamMemberDto>();
        config.NewConfig<Partner, PartnerDto>();
        config.NewConfig<Milestone, MilestoneDto>();

        config.NewConfig<ContactMessage, ContactMessageResponseDto>();

        // Request -> entity: ignore identity/audit and anti-bot fields; the service sets those explicitly.
        // Culture is used only for email templating and is never persisted.
        config.NewConfig<ContactMessageRequestDto, ContactMessage>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAtUtc)
            .Ignore(dest => dest.UpdatedAtUtc)
            .Ignore(dest => dest.IsRead);
    }
}
