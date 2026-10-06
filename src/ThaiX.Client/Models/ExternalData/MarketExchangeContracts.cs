namespace ThaiX.Client.Models.ExternalData;

public sealed record ExchangeTicker24HrResponseDto
{
    public List<ExchangeTicker24HrDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record ExchangeTicker24HrDto
{
    public string Symbol { get; init; } = string.Empty;
    public string? LastPrice { get; init; }
    public string? PriceChange { get; init; }
    public string? PriceChangePercent { get; init; }
    public string? HighPrice { get; init; }
    public string? LowPrice { get; init; }
    public string? Volume { get; init; }
    public string? QuoteVolume { get; init; }
    public string? BidPrice { get; init; }
    public string? AskPrice { get; init; }
    public long OpenTime { get; init; }
    public long CloseTime { get; init; }
}

public sealed record OrderBookDepthResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public long LastUpdateId { get; init; }
    public List<OrderBookLevelDto> Bids { get; init; } = [];
    public List<OrderBookLevelDto> Asks { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record OrderBookLevelDto
{
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
}

public sealed record FundingRatesResponseDto
{
    public List<FundingRateDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record FundingRateDto
{
    public string Symbol { get; init; } = string.Empty;
    public decimal FundingRate { get; init; }
    public long FundingTime { get; init; }
    public decimal? MarkPrice { get; init; }
}

public sealed record KlineSeriesResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public string Interval { get; init; } = string.Empty;
    public List<KlineBarDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record KlineBarDto
{
    public long OpenTime { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal Volume { get; init; }
    public long CloseTime { get; init; }
}

public sealed record StockPriceHistoryResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public List<StockPriceHistoryPointDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record StockPriceHistoryPointDto
{
    public DateOnly Date { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public long Volume { get; init; }
}

public sealed record WatchlistPriceResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public decimal LastPrice { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePercent { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public long Volume { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectEventsResponseDto
{
    public List<VnDirectEventDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectEventDto
{
    public string Code { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? EventDate { get; init; }
}

public sealed record VnDirectRatiosResponseDto
{
    public string? Code { get; init; }
    public List<VnDirectRatioItemDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectRatioItemDto
{
    public string ItemCode { get; init; } = string.Empty;
    public string? ItemName { get; init; }
    public decimal? Value { get; init; }
    public string? Unit { get; init; }
}

public sealed record VnDirectRecommendationsResponseDto
{
    public List<VnDirectRecommendationDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectRecommendationDto
{
    public string Code { get; init; } = string.Empty;
    public string Recommendation { get; init; } = string.Empty;
    public decimal? TargetPrice { get; init; }
    public string? Analyst { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed record VnDirectStockPricesResponseDto
{
    public string Code { get; init; } = string.Empty;
    public List<StockPriceHistoryPointDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectTechnicalSignalsResponseDto
{
    public string Strategy { get; init; } = string.Empty;
    public List<VnDirectTechnicalSignalDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectTechnicalSignalDto
{
    public string Code { get; init; } = string.Empty;
    public string Signal { get; init; } = string.Empty;
    public decimal? Score { get; init; }
    public DateTime? SignalTime { get; init; }
}

public sealed record TwentyFourHMoneyTransactionsResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public int Page { get; init; }
    public int PerPage { get; init; }
    public int TotalCount { get; init; }
    public List<TwentyFourHMoneyTransactionDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record TwentyFourHMoneyTransactionDto
{
    public DateTime Time { get; init; }
    public decimal Price { get; init; }
    public long Volume { get; init; }
    public string Side { get; init; } = string.Empty;
}

public sealed record Power655ResultsResponseDto
{
    public List<Power655ResultDrawDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record Power655ResultDrawDto
{
    public DateOnly DrawDate { get; init; }
    public IReadOnlyList<int> Numbers { get; init; } = [];
    public int BonusNum { get; init; }
    public long Jackpot1Value { get; init; }
    public long Jackpot2Value { get; init; }
}
