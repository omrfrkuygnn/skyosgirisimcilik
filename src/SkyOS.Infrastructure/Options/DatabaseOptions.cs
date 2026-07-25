namespace SkyOS.Infrastructure.Options;

public enum DatabaseProvider
{
    SqlServer = 0,
    Sqlite = 1,
}

/// <summary>
/// Chooses the EF Core provider at runtime. Production uses SqlServer (migrations);
/// local development may use Sqlite for zero-setup onboarding.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;
}
