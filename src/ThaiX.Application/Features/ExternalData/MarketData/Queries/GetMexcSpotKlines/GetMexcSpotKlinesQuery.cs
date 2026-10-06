using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotKlines;

/// <summary>
/// Query to retrieve MEXC spot klines by symbol and interval.
/// </summary>
public sealed record GetMexcSpotKlinesQuery : IAppQuery<MexcSpotKlinesResponse>
{
    public string Symbol { get; init; } = string.Empty;
    public string? Interval { get; init; }
}

/// <summary>
/// Response containing MEXC spot klines.
/// </summary>
public sealed record MexcSpotKlinesResponse
{
    public required string Symbol { get; init; }
    public required string Interval { get; init; }
    public required List<MexcSpotKlineDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// MEXC spot kline row mapped from array-based payload.
/// </summary>
public sealed record MexcSpotKlineDto
{
    public long OpenTime { get; init; }
    public required string OpenPrice { get; init; }
    public required string HighPrice { get; init; }
    public required string LowPrice { get; init; }
    public required string ClosePrice { get; init; }
    public required string Volume { get; init; }
    public long CloseTime { get; init; }
    public required string QuoteVolume { get; init; }
}
