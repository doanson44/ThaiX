namespace ThaiX.Client.Models.ExternalData;

/// <summary>
/// Bank interest rates response DTO for client consumption.
/// </summary>
public sealed record BankInterestRatesResponseDto
{
    public List<BankInterestRateGridItemDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Bank interest rate grid item for UI display.
/// </summary>
public sealed record BankInterestRateGridItemDto
{
    public string BankName { get; init; } = default!;
    public string Symbol { get; init; } = default!;
    public string IconUrl { get; init; } = default!;
    public decimal? Month1 { get; init; }
    public decimal? Month3 { get; init; }
    public decimal? Month6 { get; init; }
    public decimal? Month9 { get; init; }
    public decimal? Month12 { get; init; }
    public decimal? Month18 { get; init; }
    public decimal? Month24 { get; init; }
}

/// <summary>
/// Query values accepted by the bank deposit rates endpoint.
/// </summary>
public static class BankDepositRateTypes
{
    public const string Online = "online";
    public const string Offline = "offline";
}

/// <summary>
/// Bank deposit rates response DTO for client consumption.
/// </summary>
public sealed record BankDepositRatesResponseDto
{
    public string Channel { get; init; } = string.Empty;
    public List<BankDepositRateItemDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Bank deposit rate row for UI display. Rates are annual percentages; null means the term is not offered.
/// </summary>
public sealed record BankDepositRateItemDto
{
    public string BankName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? Note { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public decimal? Month1 { get; init; }
    public decimal? Month3 { get; init; }
    public decimal? Month6 { get; init; }
    public decimal? Month9 { get; init; }
    public decimal? Month12 { get; init; }
}

/// <summary>
/// Sacombank exchange rates response DTO for client consumption.
/// </summary>
public sealed record SacombankExchangeRatesResponseDto
{
    public DateTime? UpdateDate { get; init; }
    public List<SacombankExchangeRateItemDto> ExchangeRates { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Sacombank exchange rate item for UI display.
/// </summary>
public sealed record SacombankExchangeRateItemDto
{
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal BidInCash { get; init; }
    public decimal BidInTransfer { get; init; }
    public decimal OfferInCash { get; init; }
    public decimal OfferInTransfer { get; init; }
    public string? CreatedDate { get; init; }
}

/// <summary>
/// Commodities response DTO for client consumption.
/// </summary>
public sealed record CommoditiesResponseDto
{
    public List<CommodityDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record CommodityDto
{
    public string Goods { get; init; } = default!;
    public decimal Last { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePercent { get; init; }
    public string? LastUpdate { get; init; }
}

/// <summary>
/// Currencies response DTO for client consumption.
/// </summary>
public sealed record CurrenciesResponseDto
{
    public List<CurrencyDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record CurrencyDto
{
    public string ProductName { get; init; } = default!;
    public decimal CurrentPrice { get; init; }
    public decimal OtherPrice { get; init; }
    public decimal PrevPrice { get; init; }
    public decimal Change24H { get; init; }
    public decimal Change7D { get; init; }
    public string? UpdateDate { get; init; }
}

/// <summary>
/// Cryptocurrencies response DTO for client consumption.
/// </summary>
public sealed record CryptocurrenciesResponseDto
{
    public List<CryptocurrencyDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record CryptocurrencyDto
{
    public string Name { get; init; } = default!;
    public string Symbol { get; init; } = default!;
    public decimal Price { get; init; }
    public decimal MarketCap { get; init; }
    public decimal Vol24H { get; init; }
    public decimal Change24H { get; init; }
    public decimal Change7D { get; init; }
    public string? LastUpdate { get; init; }
}

/// <summary>
/// CoinGecko coins list response DTO for client consumption.
/// </summary>
public sealed record CoinGeckoCoinsListResponseDto
{
    public List<CoinGeckoCoinDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record CoinGeckoCoinDto
{
    public string Id { get; init; } = default!;
    public string Symbol { get; init; } = default!;
    public string Name { get; init; } = default!;
}

/// <summary>
/// CoinGecko coin market response DTO for client consumption.
/// </summary>
public sealed record CoinGeckoCoinMarketResponseDto
{
    public string CoinId { get; init; } = string.Empty;
    public CoinGeckoCoinMarketDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record CoinGeckoCoinMarketDto
{
    public string Id { get; init; } = default!;
    public string Symbol { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Image { get; init; }
    public decimal CurrentPrice { get; init; }
    public decimal? MarketCap { get; init; }
    public int? MarketCapRank { get; init; }
    public decimal? High24H { get; init; }
    public decimal? Low24H { get; init; }
    public decimal? PriceChangePercentage24H { get; init; }
}

/// <summary>
/// VIX response DTO for client consumption.
/// </summary>
public sealed record VixResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public decimal? CurrentPrice { get; init; }
    public decimal? PreviousClose { get; init; }
    public decimal? DayHigh { get; init; }
    public decimal? DayLow { get; init; }
    public decimal? FiftyTwoWeekHigh { get; init; }
    public decimal? FiftyTwoWeekLow { get; init; }
    public DateTime? LastUpdateTime { get; init; }
    public decimal? CurrentValue { get; init; }
    public decimal? Ema10 { get; init; }
    public decimal? Ema20 { get; init; }
    public decimal? Momentum3D { get; init; }
    public decimal? Momentum5D { get; init; }
    public double Percentile { get; init; }
    public bool IsSpike { get; init; }
    public string? Regime { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}

public sealed record GetVixVerdictRequest
{
    public required string Symbol { get; init; }
    public decimal? CurrentValue { get; init; }
    public decimal? Ema10 { get; init; }
    public decimal? Ema20 { get; init; }
    public decimal? Momentum3D { get; init; }
    public decimal? Momentum5D { get; init; }
    public double Percentile { get; init; }
    public bool IsSpike { get; init; }
    public string? Regime { get; init; }
}

public sealed record VixVerdictDto
{
    public string Commentary { get; init; } = string.Empty;
}

/// <summary>
/// VnDirect change prices response DTO for client consumption.
/// </summary>
public sealed record VnDirectChangePricesResponseDto
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectChangePriceItemDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectChangePriceItemDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal BopPrice { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePct { get; init; }
    public string LastUpdated { get; init; } = string.Empty;
}

/// <summary>
/// VnDirect top stocks response DTO for client consumption.
/// </summary>
public sealed record VnDirectTopStocksResponseDto
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectTopStockDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record VnDirectTopStockDto
{
    public string Code { get; init; } = string.Empty;
    public string Index { get; init; } = string.Empty;
    public decimal LastPrice { get; init; }
    public string LastUpdated { get; init; } = string.Empty;
    public decimal PriceChgCr1D { get; init; }
    public decimal PriceChgPctCr1D { get; init; }
    public decimal AccumulatedVal { get; init; }
    public decimal NmVolumeAvgCr20D { get; init; }
    public decimal NmVolNmVolAvg20DPctCr { get; init; }
    public decimal TotalVolumeAvgCr20D { get; init; }
    public decimal PtVolTotalVolAvg20DPctCr { get; init; }
    public decimal PtVolAvg5DTotalVolAvg20DPctCr { get; init; }
    public decimal PtVolSumCr5D { get; init; }
    public decimal PtValAvgCr5D { get; init; }
    public decimal PtVolAvgCr5D { get; init; }
    // Technical enrichment
    public string LongSignal { get; init; } = string.Empty;
    public string ShortSignal { get; init; } = string.Empty;
    public int LongBuyCount { get; init; }
    public int LongSellCount { get; init; }
    public int ShortBuyCount { get; init; }
    public int ShortSellCount { get; init; }
    // Portfolio enrichment
    public bool InTcbsCurrentHoldings { get; init; }
    public bool InTcbsAllTimeHoldings { get; init; }
    public int DragonFundCount { get; init; }
    public List<string> DragonFunds { get; init; } = [];
    public decimal DragonTotalWeight { get; init; }
    // Event enrichment
    public string EventRiskLevel { get; init; } = string.Empty;
    public int RecentEventCount { get; init; }
    public string MostSevereEventType { get; init; } = string.Empty;
    public string LatestEventEffectiveDate { get; init; } = string.Empty;
    // Scoring
    public int CompositeScore { get; init; }
    public int TechnicalScore { get; init; }
    public int PortfolioScore { get; init; }
    public int EventPenalty { get; init; }
    public int LiquidityPenalty { get; init; }
    public List<ScoreBreakdownItemDto> ScoreBreakdown { get; init; } = [];
    public bool DataCompleteness { get; init; }
}

public sealed record ScoreBreakdownItemDto
{
    public string Name { get; init; } = string.Empty;
    public int Value { get; init; }
}

/// <summary>
/// MEXC contract tickers response DTO for client consumption.
/// </summary>
public sealed record MexcContractTickersResponseDto
{
    public List<MexcContractTickerDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record MexcContractTickerDto
{
    public int? ContractId { get; set; }
    public required string Symbol { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? Bid1 { get; set; }
    public decimal? Ask1 { get; set; }
    public decimal? High24Price { get; set; }
    public decimal? Low24Price { get; set; }
    public decimal? Volume24 { get; set; }
    public decimal? Amount24 { get; set; }
    public decimal? HoldVol { get; set; }
    public decimal? RiseFallRate { get; set; }
    public decimal? RiseFallValue { get; set; }
    public decimal? IndexPrice { get; set; }
    public decimal? FairPrice { get; set; }
    public decimal? FundingRate { get; set; }
    public decimal? MaxBidPrice { get; set; }
    public decimal? MinAskPrice { get; set; }
    public object? RiseFallRates { get; set; }
    public List<decimal>? RiseFallRatesOfTimezone { get; set; }
    public long? Timestamp { get; set; }
    public decimal? CompositeScore { get; set; }
    public MexcScoreBreakdownDto? ScoreBreakdown { get; set; }
}

public sealed record MexcScoreBreakdownDto
{
    public decimal VolumeScore { get; init; }
    public decimal OpenInterestScore { get; init; }
    public decimal FundingScore { get; init; }
    public decimal MomentumScore { get; init; }
    public decimal BrokerQualityScore { get; init; }
    public decimal SocialScore { get; init; }
    public decimal FundraisingScore { get; init; }
    public decimal SupplyHealthScore { get; init; }
    public decimal UnlockPenalty { get; init; }
    public decimal FdvOverhangPenalty { get; init; }
}

/// <summary>
/// MEXC spot ticker 24h response DTO for client consumption.
/// </summary>
public sealed record MexcSpotTicker24HrResponseDto
{
    public List<MexcSpotTicker24HrDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

public sealed record MexcSpotTicker24HrDto
{
    public string Symbol { get; set; } = string.Empty;
    public string? PriceChange { get; set; }
    public string? PriceChangePercent { get; set; }
    public string? PrevClosePrice { get; set; }
    public string? LastPrice { get; set; }
    public string? BidPrice { get; set; }
    public string? BidQty { get; set; }
    public string? AskPrice { get; set; }
    public string? AskQty { get; set; }
    public string? OpenPrice { get; set; }
    public string? HighPrice { get; set; }
    public string? LowPrice { get; set; }
    public string? Volume { get; set; }
    public string? QuoteVolume { get; set; }
    public long OpenTime { get; set; }
    public long CloseTime { get; set; }
    public long? Count { get; set; }
    public decimal? CompositeScore { get; set; }
    public MexcSpotScoreBreakdownDto? ScoreBreakdown { get; set; }

    public string? Rate { get; set; }
    public string? ZonedRate { get; set; }
    public string? LastCloseRate { get; set; }
    public string? LastCloseZonedRate { get; set; }
    public string? LastCloseHigh { get; set; }
    public string? LastCloseLow { get; set; }
}

public sealed record MexcSpotScoreBreakdownDto
{
    public decimal VolumeScore { get; init; }
    public decimal MomentumScore { get; init; }
    public decimal BrokerQualityScore { get; init; }
    public decimal SocialScore { get; init; }
    public decimal FundraisingScore { get; init; }
    public decimal SupplyHealthScore { get; init; }
    public decimal UnlockPenalty { get; init; }
    public decimal FdvOverhangPenalty { get; init; }
}

// ChainBroker DTOs
public sealed record ChainBrokerFundDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = default!;
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? FundTypeName { get; init; }
    public string? FundTypeSlug { get; init; }
    public DateOnly? LastInvestmentDate { get; init; }
    public int? YearFounded { get; init; }
    public string? Status { get; init; }
    public decimal? AverageCurrentRoi { get; init; }
    public decimal? AverageMarketCapUsd { get; init; }
    public decimal? AverageInitialMarketCapUsd { get; init; }
    public decimal? AverageFdmcUsd { get; init; }
    public decimal? AverageInitialFdmcUsd { get; init; }
    public decimal? AveragePublicRaiseUsd { get; init; }
    public decimal? AveragePrivateRaiseUsd { get; init; }
    public decimal? AverageTotalRaiseUsd { get; init; }
    public decimal? AveragePriceChange24h { get; init; }
    public decimal? AveragePriceChange7d { get; init; }
    public decimal? AveragePriceChange30d { get; init; }
    public decimal? AveragePriceChange1y { get; init; }
    public int ProjectCount { get; init; }
    public int? GainersCount { get; init; }
    public decimal? GainersPercent { get; init; }
    public int? LosersCount { get; init; }
    public decimal? LosersPercent { get; init; }
}

public sealed record ChainBrokerProjectDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = default!;
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? Ticker { get; init; }
    public decimal? CurrentPriceUsd { get; init; }
    public decimal? PublicPriceUsd { get; init; }
    public decimal? PrivatePriceUsd { get; init; }
    public decimal? AthPriceUsd { get; init; }
    public decimal? PublicRoi { get; init; }
    public decimal? PrivateRoi { get; init; }
    public decimal? PublicAthRoi { get; init; }
    public decimal? PrivateAthRoi { get; init; }
    public decimal? BrokerScore { get; init; }
    public decimal? SecurityScore { get; init; }
    public int? TwitterScore { get; init; }
    public int? Rank { get; init; }
    public decimal? PublicRaiseUsd { get; init; }
    public decimal? PrivateRaiseUsd { get; init; }
    public decimal? TotalRaiseUsd { get; init; }
    public DateOnly? PrivateAnnounceDate { get; init; }
    public DateOnly? ListingDate { get; init; }
    public DateOnly? IdoDate { get; init; }
    public DateOnly? NextUnlockDate { get; init; }
    public decimal? MarketCapUsd { get; init; }
    public decimal? FdmcUsd { get; init; }
    public decimal? Volume24hUsd { get; init; }
    public decimal? CurrentCirculation { get; init; }
    public decimal? TotalCirculation { get; init; }
    public decimal? PercentCirculating { get; init; }
    public decimal? PriceChange24h { get; init; }
    public decimal? PriceChange7d { get; init; }
    public decimal? PriceChange30d { get; init; }
    public decimal? PriceChange1y { get; init; }
    public IReadOnlyList<string> Blockchains { get; init; } = [];
    public IReadOnlyList<ChainBrokerTagDto> Tags { get; init; } = [];
    public IReadOnlyList<Guid> Funds { get; init; } = [];
    public IReadOnlyList<ChainBrokerRefDto> Launchpads { get; init; } = [];
    public int FundCount => Funds.Count;
}

public sealed record ChainBrokerTagDto
{
    public string Name { get; init; } = default!;
    public string? Slug { get; init; }
}

public sealed record ChainBrokerRefDto
{
    public string Slug { get; init; } = default!;
    public string? Name { get; init; }
}

public sealed record ChainBrokerUnlockDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = default!;
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? Ticker { get; init; }
    public DateOnly? NextUnlockDate { get; init; }
    public string? UnlockAmount { get; init; }
    public decimal? UnlockValueUsd { get; init; }
    public string? RoundName { get; init; }
    public decimal? CirculationPercent { get; init; }
    public decimal? UnlockPercent { get; init; }
    public decimal? Volume24hUsd { get; init; }
    public decimal? PriceChange24h { get; init; }
    public decimal? PriceChange7d { get; init; }
    public decimal? PriceChange30d { get; init; }
    public decimal? PriceChange1y { get; init; }
}

public sealed record TcbsTop10PortfoliosResult
{
    public required IReadOnlyList<TcbsTop10PortfolioDto> Portfolios { get; init; }
    public required IReadOnlyList<string> CurrentHoldings { get; init; }
    public required IReadOnlyList<string> AllTimeHoldings { get; init; }
}

public sealed record TcbsTop10PortfolioDto
{
    public required Guid Id { get; init; }
    public required long SourceContentId { get; init; }
    public required DateOnly PostedAt { get; init; }
    public required DateOnly EffectiveDate { get; init; }
    public required IReadOnlyList<string> AddedTickers { get; init; }
    public required IReadOnlyList<string> RemovedTickers { get; init; }
    public required IReadOnlyList<TcbsTop10ImageDto> Images { get; init; }
}

public sealed record TcbsTop10ImageDto
{
    public required int ImageType { get; init; }
    public required Guid FileGuid { get; init; }
}

public sealed record DragonCapitalFundPortfolioDto
{
    public required string FundCode { get; init; }
    public DateTime? TradingDate { get; init; }
    public required IReadOnlyList<DragonCapitalAssetTypeAllocationDto> AllocationByAssetTypes { get; init; }
    public required IReadOnlyList<DragonCapitalSectorAllocationDto> AllocationBySectors { get; init; }
    public required IReadOnlyList<DragonCapitalTopHoldingDto> Top10Holdings { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}

public sealed record DragonCapitalAssetTypeAllocationDto
{
    public required string SourceName { get; init; }
    public decimal ValueAssetType { get; init; }
}

public sealed record DragonCapitalSectorAllocationDto
{
    public required string IndustryLevel2 { get; init; }
    public decimal FundWeight { get; init; }
}

public sealed record DragonCapitalTopHoldingDto
{
    public required string AssetId { get; init; }
    public decimal Weight { get; init; }
    public string? Exchange { get; init; }
    public string? IndustryLevel { get; init; }
    public string? SectorLevel { get; init; }
    public long? HoldingVolume { get; init; }
    public decimal? MarketValue { get; init; }
}

/// <summary>
/// 24HMoney transaction history response DTO for client consumption.
/// </summary>
public sealed record TwentyFourHMoneyTransactionHistoryResponseDto
{
    public string Symbol { get; init; } = string.Empty;
    public decimal? CurrentPrice { get; init; }
    public int Page { get; init; }
    public int PerPage { get; init; }
    public int TotalCount { get; init; }
    public List<PriceComparisonPeriodDto>? PriceComparison { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Volume-weighted price comparison for one look-back window.
/// "Better" = current price beats what historical buyers paid.
/// </summary>
public sealed record PriceComparisonPeriodDto
{
    public string Period { get; init; } = string.Empty;
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public long TransactionCount { get; init; }
    public long TotalVolume { get; init; }
    public long BetterVolume { get; init; }
    public long WorseVolume { get; init; }
    public long EqualVolume { get; init; }
    public decimal BetterPercent { get; init; }
    public decimal WorsePercent { get; init; }
    public decimal AveragePrice { get; init; }
    public List<PriceVolumePointDto> Top3Highest { get; init; } = [];
    public List<PriceVolumePointDto> Top3Lowest { get; init; } = [];
}

/// <summary>A price point with its aggregated matched volume.</summary>
public sealed record PriceVolumePointDto
{
    public decimal Price { get; init; }
    public long Volume { get; init; }
}
