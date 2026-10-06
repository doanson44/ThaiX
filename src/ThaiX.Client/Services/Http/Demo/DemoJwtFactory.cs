using System.Text;
using System.Text.Json;
using ThaiX.Client.Constants;

namespace ThaiX.Client.Services.Http.Demo;

/// <summary>
/// Builds an unsigned JWT whose payload is accepted by <see cref="Auth.JwtClaimsParser"/>.
/// </summary>
internal static class DemoJwtFactory
{
    public const string DemoUserId = "11111111-1111-1111-1111-111111111111";
    public const string DemoEmail = "demo-admin@local";
    public const string DemoUserName = "demo-admin";

    public static string CreateAdminToken()
    {
        var headerJson = """{"alg":"none","typ":"JWT"}""";
        var payload = new Dictionary<string, object>
        {
            ["sub"] = DemoUserId,
            ["unique_name"] = DemoUserName,
            ["email"] = DemoEmail,
            ["name"] = "Demo Admin",
            ["exp"] = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            ["scope"] = PermissionNames.GetAll().ToArray()
        };

        var payloadJson = JsonSerializer.Serialize(payload);
        return $"{Base64UrlEncode(headerJson)}.{Base64UrlEncode(payloadJson)}.demo";
    }

    private static string Base64UrlEncode(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
