using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using SkyOS.Application.Interfaces.Services;
using SkyOS.Application.Mappings;
using SkyOS.Application.Services;
using SkyOS.Application.Validators;

namespace SkyOS.Application;

/// <summary>
/// Composition root for the Application layer. Registers services, validators and mapping —
/// no Infrastructure concretes here; those bind in the Web layer's <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        MappingConfig.Register(typeAdapterConfig);
        services.AddSingleton(typeAdapterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddValidatorsFromAssemblyContaining<ContactMessageValidator>();

        services.AddScoped<IContactMessageService, ContactMessageService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IPartnerService, PartnerService>();
        services.AddScoped<IMilestoneService, MilestoneService>();
        services.AddScoped<ISiteFeedbackService, SiteFeedbackService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IContentAdminService, ContentAdminService>();
        services.AddScoped<IContactMessageAdminService, ContactMessageAdminService>();
        services.AddScoped<ISiteFeedbackAdminService, SiteFeedbackAdminService>();
        services.AddScoped<IAdminReplyService, AdminReplyService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IDashboardAdminService, DashboardAdminService>();

        return services;
    }
}
