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

    public static string UrlEncode(string text) => Uri.EscapeDataString(text);

    public static string UrlDecode(string text) => Uri.UnescapeDataString(text);

    public static string Sha256Hex(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string DecodeJwt(string token)
    {
        var parts = token.Trim().Split('.');
        if (parts.Length < 2)
            throw new FormatException("JWT must have at least header and payload segments.");

        var header = PrettyJson(Base64UrlDecode(parts[0]));
        var payload = PrettyJson(Base64UrlDecode(parts[1]));
        var sigNote = parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2])
            ? "Signature present (not verified)."
            : "Signature missing.";

        return $"// Header\n{header}\n\n// Payload\n{payload}\n\n// {sigNote}";
    }

    public static string NewUuid(bool v7) =>
        v7 ? Guid.CreateVersion7().ToString() : Guid.NewGuid().ToString();

    public static string UnixConvert(string input, bool toUnix)
    {
        if (toUnix)
        {
            if (!DateTimeOffset.TryParse(input.Trim(), out var dto))
                throw new FormatException("Invalid date/time.");
            return dto.ToUnixTimeSeconds().ToString();
        }

        if (!long.TryParse(input.Trim(), out var seconds))
            throw new FormatException("Invalid Unix timestamp.");
        // support ms if looks like ms
        var dto2 = seconds > 9_999_999_999
            ? DateTimeOffset.FromUnixTimeMilliseconds(seconds)
            : DateTimeOffset.FromUnixTimeSeconds(seconds);
        return dto2.UtcDateTime.ToString("O") + " (UTC)";
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
