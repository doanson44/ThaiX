using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Application.Common.Models.MarketScanner;

public sealed record MarketSignalResult
{
    public required string Symbol { get; init; }

    public required MarketSignalType Type { get; init; }

    public required decimal Value { get; init; }

    public required string Message { get; init; }

    public required DateTimeOffset TriggeredAtUtc { get; init; }
}
