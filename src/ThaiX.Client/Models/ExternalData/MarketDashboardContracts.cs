namespace ThaiX.Client.Models.ExternalData;

public sealed record MarketDashboardResponseDto
{
    public VixResponseDto? Vix { get; init; }
    public VnIndexAnalysisResponseDto? VnIndex { get; init; }
    public MexcBitcoinAnalysisResponseDto? Bitcoin { get; init; }
    public MexcGoldAnalysisResponseDto? Gold { get; init; }
}

public sealed record MarketAnalysisCardDto
{
    public string Title { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? Message { get; init; }
    public decimal? Price { get; init; }
    public decimal? Change { get; init; }
    public decimal? ChangePct { get; init; }
    public decimal? PreviousClose { get; init; }
    public decimal? Open { get; init; }
    public decimal? High { get; init; }
    public decimal? Low { get; init; }
    public decimal? Volume { get; init; }
    public decimal? QuoteVolume { get; init; }
    public int HistoryCount { get; init; }
    public string? LastUpdated { get; init; }
    public decimal? EmaFast { get; init; }
    public decimal? EmaMedium { get; init; }
    public decimal? EmaSlow { get; init; }
    public string EmaFastLabel { get; init; } = "EMA 9";
    public string EmaMediumLabel { get; init; } = "EMA 21";
    public string EmaSlowLabel { get; init; } = "EMA 50";
    public decimal? Rsi14 { get; init; }
    public decimal? Atr14 { get; init; }
    public decimal? AverageVolume20 { get; init; }
    public decimal? HighestHigh20 { get; init; }
    public decimal? LowestLow20 { get; init; }
    public string? Trend { get; init; }
    public string? Momentum { get; init; }
    public string? Setup { get; init; }
    public string? Signal { get; init; }
    public decimal? Confidence { get; init; }
    public decimal? Entry { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit1 { get; init; }
    public decimal? TakeProfit2 { get; init; }
    public string? Regime { get; init; }
    public decimal? Momentum3D { get; init; }
    public decimal? Momentum5D { get; init; }
    public decimal? Percentile { get; init; }
    public bool? IsSpike { get; init; }
    public IReadOnlyList<MarketAnalysisCandleDto> History { get; init; } = [];
}

public sealed record MarketAnalysisCandleDto
{
    public string DateLabel { get; init; } = string.Empty;
    public decimal Close { get; init; }
}
