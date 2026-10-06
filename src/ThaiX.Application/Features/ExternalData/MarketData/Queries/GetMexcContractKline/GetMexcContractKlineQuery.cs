using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractKline;

/// <summary>
/// Query to retrieve MEXC contract kline data by symbol and optional interval.
/// Interval defaults to Min1 when omitted.
/// </summary>
public sealed record GetMexcContractKlineQuery : IAppQuery<MexcContractKlineResponse>
{
    public string Symbol { get; init; } = string.Empty;
    public string? Interval { get; init; }
}

/// <summary>
/// Response containing MEXC contract kline data.
/// </summary>
public sealed record MexcContractKlineResponse
{
    public required string Symbol { get; init; }
    public required string Interval { get; init; }
    public MexcContractKlineDataDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// MEXC contract kline arrays.
/// </summary>
public sealed record MexcContractKlineDataDto
{
    public required List<long> Time { get; init; }
    public required List<decimal> Open { get; init; }
    public required List<decimal> Close { get; init; }
    public required List<decimal> High { get; init; }
    public required List<decimal> Low { get; init; }
    public required List<decimal> Vol { get; init; }
    public required List<decimal> Amount { get; init; }
    public required List<decimal> RealOpen { get; init; }
    public required List<decimal> RealClose { get; init; }
    public required List<decimal> RealHigh { get; init; }
    public required List<decimal> RealLow { get; init; }
}
