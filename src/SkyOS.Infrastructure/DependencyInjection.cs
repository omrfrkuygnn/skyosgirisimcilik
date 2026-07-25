using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Application.Interfaces.Persistence;
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
        IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<RecaptchaOptions>(configuration.GetSection(RecaptchaOptions.SectionName));

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
                              ?? new DatabaseOptions();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<SkyOSDbContext>(options =>
        {
            switch (databaseOptions.Provider)
            {
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(
                        connectionString ?? "Data Source=skyos.dev.db",
                        sqlite => sqlite.MigrationsAssembly(typeof(SkyOSDbContext).Assembly.FullName));
                    break;

                case DatabaseProvider.SqlServer:
                default:
                    options.UseSqlServer(
                        connectionString,
                        sql => sql.MigrationsAssembly(typeof(SkyOSDbContext).Assembly.FullName));
                    break;
            }
        });

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
