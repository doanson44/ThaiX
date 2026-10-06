namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Normalizes phone numbers for storage and lookup.
/// Rules: remove spaces/dots/dashes; +84xxxxxxxxx and 0xxxxxxxxx -> 84xxxxxxxxx; digits only.
/// </summary>
public static class PhoneNormalizer
{
    private const string VietnamCountryCode = "84";

    /// <summary>
    /// Normalizes a phone number to digits only (E.164-style without plus).
    /// Vietnamese: +84/0 prefix -> 84. Returns null if input is null/empty or has no digits.
    /// </summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
            return null;

        // Vietnamese: 0xxxxxxxxx -> 84xxxxxxxxx
        if (digits.StartsWith("0", StringComparison.Ordinal) && digits.Length >= 9)
            digits = VietnamCountryCode + digits[1..];

        // +84 already: strip leading 0 if present after 84
        if (digits.StartsWith(VietnamCountryCode, StringComparison.Ordinal) && digits.Length > VietnamCountryCode.Length)
            return digits;

        if (digits.StartsWith("+", StringComparison.Ordinal))
            digits = digits[1..];

        return digits.Length > 0 ? digits : null;
    }
}
