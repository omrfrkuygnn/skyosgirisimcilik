using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
using SkyOS.Infrastructure.Identity;
using SkyOS.Infrastructure.Options;
using SkyOS.Infrastructure.Persistence;
using SkyOS.Infrastructure.Repositories;
using SkyOS.Infrastructure.Services;

namespace SkyOS.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer: DbContext + provider selection,
/// repositories/unit of work, and external service implementations (SMTP, reCAPTCHA, clock).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? contentRootPath = null)
    {
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<RecaptchaOptions>(configuration.GetSection(RecaptchaOptions.SectionName));
        services.Configure<BackofficeOptions>(configuration.GetSection(BackofficeOptions.SectionName));
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SkyOSDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(SkyOSDbContext).Assembly.FullName));
        });

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<SkyOSDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IEmailSender, EmailSender>();

        services.AddHttpClient<IRecaptchaValidator, RecaptchaValidator>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
