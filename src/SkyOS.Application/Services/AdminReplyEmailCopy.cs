namespace SkyOS.Application.Services;

internal static class AdminReplyEmailCopy
{
    internal sealed record Labels(
        string Lang,
        string Heading,
        string Greeting,
        string OriginalHeading,
        string Footer);

    internal static Labels ForCulture(string? culture) =>
        Normalize(culture) switch
        {
            "de" => German,
            "en" => English,
            _ => Turkish,
        };

    private static string Normalize(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return "tr";
        }

        var two = culture.Length >= 2 ? culture[..2].ToLowerInvariant() : culture.ToLowerInvariant();
        return two is "en" or "tr" or "de" ? two : "tr";
    }

    private static Labels Turkish => new(
        Lang: "tr",
        Heading: "SkyOS Ekibinden Yanıt",
        Greeting: "Merhaba",
        OriginalHeading: "SİZİN MESAJINIZ",
        Footer: "Bu e-posta SkyOS ekibi tarafından gönderilmiştir. Yanıtlamak için bu mesaja doğrudan cevap verebilirsiniz.");

    private static Labels English => new(
        Lang: "en",
        Heading: "Reply from the SkyOS Team",
        Greeting: "Hello",
        OriginalHeading: "YOUR ORIGINAL MESSAGE",
        Footer: "This message was sent by the SkyOS team. You can reply directly to this email.");

    private static Labels German => new(
        Lang: "de",
        Heading: "Antwort vom SkyOS-Team",
        Greeting: "Hallo",
        OriginalHeading: "IHRE URSPRÜNGLICHE NACHRICHT",
        Footer: "Diese Nachricht wurde vom SkyOS-Team gesendet. Sie können direkt auf diese E-Mail antworten.");
}
