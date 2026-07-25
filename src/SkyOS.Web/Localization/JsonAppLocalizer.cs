using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkyOS.Shared.Localization;

namespace SkyOS.Web.Localization;

/// <summary>
/// Loads and caches flattened key/value maps from Resources/Locales/{culture}.json.
/// </summary>
public sealed class LocaleCatalog
{
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _localesPath;
    private readonly ILogger<LocaleCatalog> _logger;

    public LocaleCatalog(IHostEnvironment env, ILogger<LocaleCatalog> logger)
    {
        _logger = logger;
        _localesPath = Path.Combine(env.ContentRootPath, "Resources", "Locales");
    }

    public IReadOnlyDictionary<string, string> GetFlatMap(string culture)
    {
        var key = Normalize(culture);
        return _cache.GetOrAdd(key, Load);
    }

    private IReadOnlyDictionary<string, string> Load(string culture)
    {
        var path = Path.Combine(_localesPath, $"{culture}.json");
        if (!File.Exists(path))
        {
            _logger.LogWarning("Locale file missing: {Path}. Falling back to empty map.", path);
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        using var stream = File.OpenRead(path);
        using var doc = JsonDocument.Parse(stream);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Flatten(doc.RootElement, prefix: null, map);
        _logger.LogInformation("Loaded locale '{Culture}' with {Count} keys from {Path}.", culture, map.Count, path);
        return map;
    }

    private static void Flatten(JsonElement element, string? prefix, IDictionary<string, string> map)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in element.EnumerateObject())
                {
                    var next = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
                    Flatten(prop.Value, next, map);
                }

                break;

            case JsonValueKind.Array:
                var i = 0;
                foreach (var item in element.EnumerateArray())
                {
                    Flatten(item, $"{prefix}[{i}]", map);
                    i++;
                }

                break;

            case JsonValueKind.String:
                if (prefix is not null)
                {
                    map[prefix] = element.GetString() ?? string.Empty;
                }

                break;

            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                if (prefix is not null)
                {
                    map[prefix] = element.ToString();
                }

                break;
        }
    }

    public static string Normalize(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return "tr";
        }

        var two = culture.Length >= 2 ? culture[..2].ToLowerInvariant() : culture.ToLowerInvariant();
        return two is "en" or "tr" or "de" ? two : "tr";
    }
}

/// <summary>
/// Request-scoped localizer that resolves keys against the current UI culture.
/// </summary>
public sealed class JsonAppLocalizer : IAppLocalizer
{
    private readonly LocaleCatalog _catalog;

    public JsonAppLocalizer(LocaleCatalog catalog)
    {
        _catalog = catalog;
    }

    public string Culture => LocaleCatalog.Normalize(CultureInfo.CurrentUICulture.Name);

    public string this[string key] => Get(key);

    public string this[string key, params object[] args] => Get(key, args);

    public string Get(string key, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var map = _catalog.GetFlatMap(Culture);
        if (!map.TryGetValue(key, out var value))
        {
            // Fallback to Turkish, then the key itself.
            if (Culture != "tr")
            {
                var tr = _catalog.GetFlatMap("tr");
                if (tr.TryGetValue(key, out value))
                {
                    return Format(value, args);
                }
            }

            return key;
        }

        return Format(value, args);
    }

    private static string Format(string value, object[] args)
    {
        if (args.Length == 0)
        {
            return value;
        }

        try
        {
            return string.Format(CultureInfo.CurrentUICulture, value, args);
        }
        catch (FormatException)
        {
            return value;
        }
    }
}
