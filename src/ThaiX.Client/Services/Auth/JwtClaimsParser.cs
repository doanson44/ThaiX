using System.Security.Claims;
using System.Text.Json;

namespace ThaiX.Client.Services.Auth;

internal static class JwtClaimsParser
{
    public static IReadOnlyList<Claim> ParseClaims(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return Array.Empty<Claim>();
        }

        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return Array.Empty<Claim>();
        }

        try
        {
            var jsonBytes = ParseBase64WithoutPadding(parts[1]);
            using var document = JsonDocument.Parse(jsonBytes);

            var claims = new List<Claim>();
            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        claims.Add(new Claim(property.Name, item.ToString()));
                    }

                    continue;
                }

                claims.Add(new Claim(property.Name, property.Value.ToString()));
            }

            return claims;
        }
        catch
        {
            return Array.Empty<Claim>();
        }
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        var padded = (base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            _ => base64
        };

        padded = padded.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(padded);
    }
}
