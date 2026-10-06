using System.Text.Json.Serialization;

namespace ThaiX.Infrastructure.ExternalApis.DTOs;

/// <summary>
/// Bybit API response wrapper.
/// </summary>
/// <typeparam name="T">Result type.</typeparam>
public sealed class BybitApiResponse<T>
{
    [JsonPropertyName("retCode")]
    public int RetCode { get; set; }

    [JsonPropertyName("retMsg")]
    public string? RetMsg { get; set; }

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("time")]
    public long Time { get; set; }
}

/// <summary>
/// Bybit tickers result wrapper.
/// </summary>
/// <typeparam name="T">Ticker type.</typeparam>
public sealed class BybitTickersResult<T>
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("list")]
    public List<T>? List { get; set; }
}
