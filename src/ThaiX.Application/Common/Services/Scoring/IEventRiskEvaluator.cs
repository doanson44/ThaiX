using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;

namespace ThaiX.Application.Common.Services.Scoring;

public interface IEventRiskEvaluator
{
    EventRiskResult Evaluate(IReadOnlyList<VnDirectEventDto> events, int lookbackDays);
}

public sealed record EventRiskResult
{
    public required string Level { get; init; }
    public int Penalty { get; init; }
    public required string MostSevereType { get; init; }
    public required string LatestEffectiveDate { get; init; }
    public int Count { get; init; }
}
