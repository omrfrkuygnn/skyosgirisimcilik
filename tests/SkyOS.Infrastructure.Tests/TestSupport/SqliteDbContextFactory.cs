using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SkyOS.Application.Interfaces.Infrastructure;
using SkyOS.Infrastructure.Persistence;

namespace SkyOS.Infrastructure.Tests.TestSupport;

/// <summary>
/// Builds a real <see cref="SkyOSDbContext"/> backed by an in-memory SQLite database.
/// Using SQLite (rather than the EF InMemory provider) exercises the real relational
/// query pipeline, indexes and seed data.
/// </summary>
public sealed class SqliteDbContextFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = Create();
        context.Database.EnsureCreated();
    }

    public SkyOSDbContext Create()
    {
        var options = new DbContextOptionsBuilder<SkyOSDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new SkyOSDbContext(options, new FixedDateTimeProvider());
    }

    public void Dispose() => _connection.Dispose();

    private sealed class FixedDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = new(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
    }
}
