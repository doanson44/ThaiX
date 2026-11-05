using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ThaiX.Client.Services.Tools.Transforms;

public static class EncodeToolTransforms
{
    public static string Base64Encode(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    public static string Base64Decode(string text) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(text.Trim()));

    public static string Base64UrlEncode(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? string.Empty))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    public static string Base64UrlDecodeText(string text) =>
        Base64UrlDecode((text ?? string.Empty).Trim());

    public static string UrlEncode(string text)
    {
        var value = text ?? string.Empty;
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            return uri.AbsoluteUri;

        return Uri.EscapeDataString(value);
    }

    public static string UrlDecode(string text) => Uri.UnescapeDataString(text);

    public static string UrlEncodeComponent(string text) =>
        Uri.EscapeDataString(text ?? string.Empty);

    public static string UrlDecodeComponent(string text) =>
        Uri.UnescapeDataString(text ?? string.Empty);

    public static string Sha256Hex(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string DecodeJwt(
        string token,
        string warning = "UNVERIFIED: signature is not checked. Treat all claims as untrusted.",
        string headerLabel = "Header",
        string payloadLabel = "Payload",
        string signaturePresentLabel = "Signature present (not verified).",
        string signatureMissingLabel = "Signature missing.")
    {
        var parts = token.Trim().Split('.');
        if (parts.Length < 2)
            throw new FormatException("JWT must have at least header and payload segments.");

        var header = PrettyJson(Base64UrlDecode(parts[0]));
        var payload = PrettyJson(Base64UrlDecode(parts[1]));
        var signatureNote = parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2])
            ? signaturePresentLabel
            : signatureMissingLabel;

        return $"{warning}\n\n// {headerLabel}\n{header}\n\n// {payloadLabel}\n{payload}\n\n// {signatureNote}";
    }

    public static string NewUuid(bool v7, string format = "D")
    {
        var normalizedFormat = string.IsNullOrWhiteSpace(format) ? "D" : format.Trim().ToUpperInvariant();
        if (normalizedFormat is not ("D" or "N" or "B" or "P" or "X"))
            throw new FormatException("UUID format must be D, N, B, P, or X.");

        var value = v7 ? Guid.CreateVersion7() : Guid.NewGuid();
        return value.ToString(normalizedFormat);
    }

    public static string UnixConvert(string input, bool toUnix, bool milliseconds = false)
    {
        if (toUnix)
        {
            if (!DateTimeOffset.TryParse(
                    input.Trim(),
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var dateTime))
                throw new FormatException("Invalid date/time.");

            return (milliseconds ? dateTime.ToUnixTimeMilliseconds() : dateTime.ToUnixTimeSeconds())
                .ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (!long.TryParse(input.Trim(), System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var timestamp))
            throw new FormatException("Invalid Unix timestamp.");

        try
        {
            var dateTime = milliseconds
                ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                : DateTimeOffset.FromUnixTimeSeconds(timestamp);
            return dateTime.UtcDateTime.ToString("O", System.Globalization.CultureInfo.InvariantCulture) + " (UTC)";
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new FormatException("Unix timestamp is outside the supported range.");
        }
    }

    private static string Base64UrlDecode(string segment)
    {
        var s = segment.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(s));
    }

    private static string PrettyJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }
}
