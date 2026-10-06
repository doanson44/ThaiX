using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;

namespace ThaiX.Application.Common.Services.Scoring;

public sealed record StockScoringContext
{
    public required string Code { get; init; }

    /// <summary>VnDirect average liquidity inputs used only for liquidity penalty.</summary>
    public decimal TotalVolumeAvgCr20D { get; init; }

    public decimal NmVolumeAvgCr20D { get; init; }

    public decimal NmVolNmVolAvg20DPctCr { get; init; }

    public VnDirectTechnicalSignalDto? LongSignal { get; init; }
    public VnDirectTechnicalSignalDto? ShortSignal { get; init; }
    public bool IsInTcbsHoldings { get; init; }
    public required DragonHoldingMetrics DragonMetrics { get; init; }
    public required IReadOnlyList<VnDirectEventDto> Events { get; init; }
    public required int EventLookbackDays { get; init; }
}
