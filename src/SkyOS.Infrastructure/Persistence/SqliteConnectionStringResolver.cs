using SkyOS.Infrastructure.Options;

namespace SkyOS.Infrastructure.Persistence;

internal static class SqliteConnectionStringResolver
{
    public static string? Resolve(string? connectionString, DatabaseProvider provider, string? contentRootPath)
    {
        if (provider != DatabaseProvider.Sqlite || string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        const string prefix = "Data Source=";
        if (!connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var dataSource = connectionString[prefix.Length..].Trim().Trim('"');
        if (Path.IsPathRooted(dataSource) || string.IsNullOrWhiteSpace(contentRootPath))
        {
            return connectionString;
        }

        var absolutePath = Path.GetFullPath(Path.Combine(contentRootPath, dataSource));
        return $"{prefix}{absolutePath}";
    }
}
