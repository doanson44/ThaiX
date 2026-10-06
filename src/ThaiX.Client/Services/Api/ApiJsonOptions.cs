using System.Text.Json;
using System.Text.Json.Serialization;

namespace ThaiX.Client.Services.Api;

/// <summary>
/// Shared JSON serializer options matching the server's serialization settings.
/// </summary>
public static class ApiJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
