using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
                              ?? new DatabaseOptions();
        var connectionString = SqliteConnectionStringResolver.Resolve(
            configuration.GetConnectionString("DefaultConnection"),
            databaseOptions.Provider,
            contentRootPath);

        services.AddDbContext<SkyOSDbContext>(options =>
        {
            switch (databaseOptions.Provider)
            {
                case DatabaseProvider.Sqlite:
                    options.UseSqlite(
                        connectionString ?? "Data Source=skyos.dev.db",
                        sqlite => sqlite.MigrationsAssembly(typeof(SkyOSDbContext).Assembly.FullName))
                        // Migrations are authored against SQL Server; SQLite dev may report false pending-model diffs in EF Core 9.
                        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
                    break;

                case DatabaseProvider.SqlServer:
                default:
                    options.UseSqlServer(
                        connectionString,
                        sql => sql.MigrationsAssembly(typeof(SkyOSDbContext).Assembly.FullName));
                    break;
            }
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
