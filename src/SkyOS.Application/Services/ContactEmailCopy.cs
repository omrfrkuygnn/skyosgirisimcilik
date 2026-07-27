using SkyOS.Domain.Enums;

namespace SkyOS.Application.Services;

internal static class ContactEmailCopy
{
    internal sealed record Labels(
        string Lang,
        string SubjectPrefix,
        string Heading,
        string BadgeHeading,
        string LabelName,
        string LabelCompany,
        string LabelEmail,
        string LabelPhone,
        string LabelTopic,
        string LabelMessage,
        string FooterNote,
        string ReplyLabel,
        string DateLabel,
        string MessageIdLabel,
        string IpLabel,
        string PlainHeading);

    internal static Labels ForCulture(string? culture) =>
        Normalize(culture) switch
        {
            "tr" => Turkish,
            "de" => German,
            _ => English,
        };

    internal static string InterestLabel(InterestType interestType, string? culture) =>
        (Normalize(culture), interestType) switch
        {
            ("tr", InterestType.Yatirimci) => "Yatırımcı",
            ("tr", InterestType.KurumsalIsBirligi) => "Kurumsal İş Birliği",
            ("tr", InterestType.Basin) => "Basın",
            ("tr", InterestType.Diger) => "Diğer",
            ("de", InterestType.Yatirimci) => "Investor",
            ("de", InterestType.KurumsalIsBirligi) => "Unternehmenspartnerschaft",
            ("de", InterestType.Basin) => "Presse",
            ("de", InterestType.Diger) => "Sonstiges",
            (_, InterestType.Yatirimci) => "Investor",
            (_, InterestType.KurumsalIsBirligi) => "Corporate Partnership",
            (_, InterestType.Basin) => "Press",
            (_, InterestType.Diger) => "Other",
            _ => interestType.ToString(),
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
        SubjectPrefix: "SkyOS İletişim",
        Heading: "Yeni İletişim Mesajı",
        BadgeHeading: "YENİ İLETİŞİM MESAJI",
        LabelName: "Ad Soyad",
        LabelCompany: "Kurum / Şirket",
        LabelEmail: "E-posta Adresi",
        LabelPhone: "Telefon Numarası",
        LabelTopic: "İLGİ ALANI",
        LabelMessage: "GELEN MESAJ",
        FooterNote: "Bu e-posta SkyOS web platformu iletişim formu üzerinden otomatik oluşturulmuştur.",
        ReplyLabel: "Göndereni E-posta ile Yanıtla",
        DateLabel: "Tarih",
        MessageIdLabel: "Mesaj ID",
        IpLabel: "Gönderen IP",
        PlainHeading: "YENİ İLETİŞİM MESAJI");

    private static Labels English => new(
        Lang: "en",
        SubjectPrefix: "SkyOS Contact",
        Heading: "New Contact Form Submission",
        BadgeHeading: "NEW CONTACT SUBMISSION",
        LabelName: "Full Name",
        LabelCompany: "Company / Organization",
        LabelEmail: "Email Address",
        LabelPhone: "Phone Number",
        LabelTopic: "TOPIC",
        LabelMessage: "SUBMITTED MESSAGE",
        FooterNote: "This notification was automatically generated via the SkyOS web platform contact form.",
        ReplyLabel: "Reply to Sender via Email",
        DateLabel: "Date",
        MessageIdLabel: "Message ID",
        IpLabel: "Sender IP",
        PlainHeading: "NEW CONTACT SUBMISSION");

    private static Labels German => new(
        Lang: "de",
        SubjectPrefix: "SkyOS Kontakt",
        Heading: "Neue Kontaktanfrage",
        BadgeHeading: "NEUE KONTAKTANFRAGE",
        LabelName: "Name",
        LabelCompany: "Unternehmen / Organisation",
        LabelEmail: "E-Mail-Adresse",
        LabelPhone: "Telefonnummer",
        LabelTopic: "THEMA",
        LabelMessage: "EINGEGANGENE NACHRICHT",
        FooterNote: "Diese Benachrichtigung wurde automatisch über das Kontaktformular der SkyOS-Webplattform erstellt.",
        ReplyLabel: "Absender per E-Mail antworten",
        DateLabel: "Datum",
        MessageIdLabel: "Nachrichten-ID",
        IpLabel: "Absender-IP",
        PlainHeading: "NEUE KONTAKTANFRAGE");
}
