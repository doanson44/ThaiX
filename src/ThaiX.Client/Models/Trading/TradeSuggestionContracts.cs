namespace ThaiX.Client.Models.Trading;

public enum MarketType
{
    Stock = 1,
    Crypto = 2,
    CryptoSpot = 3
}

public enum SuggestionAssetClass
{
    Stock = 1,
    Crypto = 2
}

public enum SuggestionReportType
{
    TopSuggestionsWeekly = 1
}

public enum WeeklySuggestionReportStatus
{
    Succeeded = 1,
    PartialSucceeded = 2,
    NoData = 3,
    Failed = 4
}

public enum MarketRegime
{
    Neutral = 0,
    RiskOn = 1,
    RiskOff = 2
}

public enum EventRisk
{
    Low = 0,
    Medium = 1,
    High = 2
}

public sealed record GetTradeSuggestionRequest
{
    public required string Symbol { get; init; }
    public required MarketType MarketType { get; init; }
    public required string Timeframe { get; init; }
    public bool UseLongTermTimeframe { get; init; }
    public MarketRegime MarketRegime { get; init; } = MarketRegime.RiskOn;
    public EventRisk EventRisk { get; init; } = EventRisk.Low;
}

public sealed record TradeSuggestionDto
{
    public required string Trend { get; init; }
    public required string Momentum { get; init; }
    public required string Setup { get; init; }
    public required string Signal { get; init; }
    public decimal? EntryPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit1 { get; init; }
    public decimal? TakeProfit2 { get; init; }
    public decimal Confidence { get; init; }
}

public sealed record GetTradeVerdictRequest
{
    public required string Symbol { get; init; }
    public required string MarketType { get; init; }
    public required string Timeframe { get; init; }
    public required string Trend { get; init; }
    public required string Momentum { get; init; }
    public required string Setup { get; init; }
    public required string Signal { get; init; }
    public decimal? EntryPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit1 { get; init; }
    public decimal? TakeProfit2 { get; init; }
    public decimal Confidence { get; init; }
}

public sealed record TradeVerdictDto
{
    public string Verdict { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
}

public sealed record WeeklySuggestionHistoryResultDto
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required IReadOnlyList<WeeklySuggestionHistoryReportDto> Items { get; init; }
}

public sealed record WeeklySuggestionTemplateFileDto
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] FileContent { get; init; }
}

public sealed record WeeklySuggestionHistoryReportDto
{
    public required Guid ReportId { get; init; }
    public required string ReportKey { get; init; }
    public required DateTime RunAtUtc { get; init; }
    public required SuggestionAssetClass AssetClass { get; init; }
    public required SuggestionReportType ReportType { get; init; }
    public required WeeklySuggestionReportStatus Status { get; init; }
    public required int PickedCount { get; init; }
    public required IReadOnlyList<WeeklySuggestionHistoryItemDto> Picks { get; init; }
}

public sealed record WeeklySuggestionHistoryItemDto
{
    public required string Timeframe { get; init; }
    public required int Rank { get; init; }
    public required string Symbol { get; init; }
    public required SuggestionAssetClass MarketType { get; init; }
    public required decimal EntryPrice { get; init; }
    public required string Signal { get; init; }
    public required decimal Confidence { get; init; }
}

public sealed record EvaluateWeeklySuggestionPerformanceRequestDto
{
    public SuggestionAssetClass? AssetClass { get; init; }
    public int? LookbackReports { get; init; }
    public required IReadOnlyList<EvaluateWeeklySuggestionSymbolInputDto> Symbols { get; init; }
}

public sealed record EvaluateWeeklySuggestionSymbolInputDto
{
    public required string Symbol { get; init; }
    public decimal? CurrentPrice { get; init; }
}

public sealed record EvaluateWeeklySuggestionPerformanceResultDto
{
    public required int MatchedCount { get; init; }
    public required IReadOnlyList<EvaluatedWeeklySuggestionDto> Items { get; init; }
}

public sealed record EvaluatedWeeklySuggestionDto
{
    public required string Symbol { get; init; }
    public required DateTime RunAtUtc { get; init; }
    public required string Timeframe { get; init; }
    public required int Rank { get; init; }
    public required decimal EntryPrice { get; init; }
    public decimal? CurrentPrice { get; init; }
    public decimal? ChangePercentFromEntry { get; init; }
    public required string ReportKey { get; init; }
    public required SuggestionAssetClass AssetClass { get; init; }
}
