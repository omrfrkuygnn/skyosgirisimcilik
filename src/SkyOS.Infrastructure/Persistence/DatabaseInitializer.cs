using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SkyOS.Infrastructure.Persistence;

/// <summary>
/// Applies the schema on startup. The migrations are authored for SQL Server, so we only run
/// <c>Migrate()</c> on that provider. For the SQLite dev fallback we use <c>EnsureCreated()</c>,
/// which generates a correct SQLite schema from the model (seed data is applied either way).
/// </summary>
public static class DatabaseInitializer
{
    private const string SqlServerProviderName = "Microsoft.EntityFrameworkCore.SqlServer";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");
        var context = provider.GetRequiredService<SkyOSDbContext>();

        try
        {
            if (string.Equals(context.Database.ProviderName, SqlServerProviderName, StringComparison.Ordinal))
            {
                await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("SQL Server migrations applied successfully.");
            }
            else
            {
                await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
                logger.LogInformation("Database schema ensured via EnsureCreated for provider {Provider}.", context.Database.ProviderName);

                // Auto-patch SQLite schema silently if columns were added after initial SQLite DB file creation
                var connection = context.Database.GetDbConnection();
                var wasOpen = connection.State == System.Data.ConnectionState.Open;
                if (!wasOpen)
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }

                try
                {
                    using var command = connection.CreateCommand();
                    command.CommandText = "PRAGMA table_info(Partners);";
                    using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        existingColumns.Add(reader.GetString(1));
                    }
                    reader.Close();

                    if (!existingColumns.Contains("Address"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Partners ADD COLUMN Address TEXT;", cancellationToken).ConfigureAwait(false);
                    }
                    if (!existingColumns.Contains("Phone"))
                    {
                        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Partners ADD COLUMN Phone TEXT;", cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (!wasOpen)
                    {
                        await connection.CloseAsync().ConfigureAwait(false);
                    }
                }

                // Ensure Harven Mühendislik seed entry exists in existing SQLite database
                var harven = await context.Partners.FirstOrDefaultAsync(p => p.Id == 2 || p.Name.Contains("Harven"), cancellationToken).ConfigureAwait(false);
                if (harven is null)
                {
                    context.Partners.Add(new Domain.Entities.Partner
                    {
                        Id = 2,
                        Name = "Harven Mühendislik",
                        LogoUrl = "/img/partners/harven-logo.png",
                        Description = "SkyOS otonom sistemler girişim projemizin teknik altyapı, mühendislik danışmanlığı ve saha uygulamalarında projeyi destekleyen çözüm ortağımız.",
                        Address = "Çankaya Mahallesi Cinnah Caddesi Erim İş Hanı No:37/22, 06690 Çankaya/Ankara",
                        Phone = "(0312) 438 22 23",
                        IsVerifiedRelationship = true,
                        DisplayOrder = 2,
                        CreatedAtUtc = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                else if (string.IsNullOrEmpty(harven.Address))
                {
                    harven.Address = "Çankaya Mahallesi Cinnah Caddesi Erim İş Hanı No:37/22, 06690 Çankaya/Ankara";
                    harven.Phone = "(0312) 438 22 23";
                    harven.LogoUrl = "/img/partners/harven-logo.png";
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed.");
            throw;
        }
    }
}
