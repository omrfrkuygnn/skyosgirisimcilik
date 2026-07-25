using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SkyOS.Infrastructure.Services;

namespace SkyOS.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core CLI (<c>dotnet ef migrations</c> / <c>database update</c>).
/// Migrations are authored against SQL Server (the production provider). The connection string can
/// be overridden with the SKYOS_MIGRATION_CONNECTION environment variable.
/// </summary>
public sealed class SkyOSDbContextFactory : IDesignTimeDbContextFactory<SkyOSDbContext>
{
    private const string DefaultConnection =
        "Server=(localdb)\\mssqllocaldb;Database=SkyOS;Trusted_Connection=True;MultipleActiveResultSets=true";

    public SkyOSDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("SKYOS_MIGRATION_CONNECTION") ?? DefaultConnection;

        var options = new DbContextOptionsBuilder<SkyOSDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SkyOSDbContextFactory).Assembly.FullName))
            .Options;

        return new SkyOSDbContext(options, new SystemDateTimeProvider());
    }
}
