namespace SkyOS.Shared.Extensions;

/// <summary>
/// String helpers, primarily for masking PII before it reaches logs (KVKK / GDPR compliance).
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Masks an e-mail address for logging: <c>john.doe@example.com</c> becomes <c>j***@example.com</c>.
    /// Never logs the full local part.
    /// </summary>
    public static string MaskEmail(this string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return "***";
        }

        var firstChar = email[0];
        var domain = email[atIndex..];
        return $"{firstChar}***{domain}";
    }

    /// <summary>
    /// Masks a phone number for logging, keeping only the last two digits: <c>05334990122</c> -&gt; <c>*********22</c>.
    /// </summary>
    public static string MaskPhone(this string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length <= 2)
        {
            return new string('*', digitsOnly.Length);
        }

        var visible = digitsOnly[^2..];
        return new string('*', digitsOnly.Length - 2) + visible;
    }

    public static bool HasValue(this string? value) => !string.IsNullOrWhiteSpace(value);
}
