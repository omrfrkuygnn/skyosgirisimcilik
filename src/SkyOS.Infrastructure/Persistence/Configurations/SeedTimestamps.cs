namespace SkyOS.Infrastructure.Persistence.Configurations;

/// <summary>
/// A fixed timestamp for HasData seeding. Must be constant so EF does not detect a model
/// change on every build (which would create spurious migrations).
/// </summary>
internal static class SeedTimestamps
{
    public static readonly DateTime Initial = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
