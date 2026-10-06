namespace ThaiX.Application.Common.Configuration;

/// <summary>
/// Thresholds and point penalties for thinning composite score when average traded volume is weak.
/// Tune using VnDirect field units for your environment.
/// </summary>
public sealed class LiquidityScoringOptions
{
    public const string SectionName = "LiquidityScoring";

    /// <summary>Master switch; when false no liquidity penalty is applied.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Penalty when max(TotalVolumeAvgCr20D, NmVolumeAvgCr20D) is at or above this level.</summary>
    public decimal AdequateVolumeAvgThreshold { get; init; } = 250_000m;

    /// <summary>Penalty when effective volume is below adequate but at or above moderate.</summary>
    public decimal ModerateVolumeAvgThreshold { get; init; } = 80_000m;

    /// <summary>Penalty when effective volume is below moderate but at or above thin.</summary>
    public decimal ThinVolumeAvgThreshold { get; init; } = 25_000m;

    public int PenaltyAdequateOrBetter { get; init; } = 0;

    public int PenaltyBetweenModerateAndAdequate { get; init; } = -6;

    public int PenaltyBetweenThinAndModerate { get; init; } = -14;

    public int PenaltyBelowThinButPositive { get; init; } = -24;

    /// <summary>When both averaged volume metrics are zero (illiquid or missing liquidity data).</summary>
    public int PenaltyNoVolumeSignal { get; init; } = -32;

    /// <summary>
    /// NM volume as % of its 20-day average. When between 0 exclusive and this threshold, extra penalty applies.
    /// Set to 0 to disable.
    /// </summary>
    public decimal RelativeNmVolumePoorPctThreshold { get; init; } = 40m;

    public int RelativeNmVolumePoorExtraPenalty { get; init; } = -8;

    /// <summary>Most negative cumulative liquidity penalty allowed (e.g. -40).</summary>
    public int MaxLiquidityPenalty { get; init; } = -40;
}
