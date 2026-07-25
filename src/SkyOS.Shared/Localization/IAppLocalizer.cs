namespace SkyOS.Shared.Localization;

/// <summary>
/// Culture-aware string lookup backed by JSON locale files (tr.json / en.json).
/// Keys use dotted paths, e.g. <c>Nav.Home</c>.
/// </summary>
public interface IAppLocalizer
{
    string this[string key] { get; }

    string this[string key, params object[] args] { get; }

    string Get(string key, params object[] args);

    string Culture { get; }
}
