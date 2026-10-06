using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.Trading;

namespace ThaiX.Client.Services.ExternalData;

/// <summary>
/// Client service for external data API endpoints.
/// </summary>
public interface IExternalDataService
{
    Task<ApiResponse<BankInterestRatesResponseDto>> GetBankInterestRatesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<BankDepositRatesResponseDto>> GetBankDepositRatesAsync(string type, CancellationToken cancellationToken = default);
    Task<ApiResponse<SacombankExchangeRatesResponseDto>> GetSacombankExchangeRatesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CommoditiesResponseDto>> GetCommoditiesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CurrenciesResponseDto>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CryptocurrenciesResponseDto>> GetCryptocurrenciesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CoinGeckoCoinsListResponseDto>> GetCoinGeckoCoinsListAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CoinGeckoCoinMarketResponseDto>> GetCoinGeckoCoinMarketByIdAsync(string coinId, CancellationToken cancellationToken = default);
    Task<ApiResponse<VixResponseDto>> GetYahooFinanceVixAsync(CancellationToken cancellationToken = default);
    Task<VixVerdictDto> GetVixVerdictAsync(GetVixVerdictRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectChangePricesResponseDto>> GetVnDirectChangePricesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectTopStocksResponseDto>> GetVnDirectTopStocksAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<MexcContractTickersResponseDto>> GetMexcContractTickersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<MexcSpotTicker24HrResponseDto>> GetMexcSpotTicker24HrAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderBookDepthResponseDto>> GetMexcContractDepthAsync(string symbol, CancellationToken cancellationToken = default);
    Task<ApiResponse<KlineSeriesResponseDto>> GetMexcContractKlineAsync(string symbol, string? interval = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<FundingRatesResponseDto>> GetMexcFundingRatesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderBookDepthResponseDto>> GetMexcSpotDepthAsync(string symbol, CancellationToken cancellationToken = default);
    Task<ApiResponse<KlineSeriesResponseDto>> GetMexcSpotKlinesAsync(string symbol, string? interval = null, CancellationToken cancellationToken = default);

    Task<ApiResponse<FundingRatesResponseDto>> GetBinanceFundingRatesAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderBookDepthResponseDto>> GetBinanceFuturesDepthAsync(string symbol, CancellationToken cancellationToken = default);
    Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBinanceFuturesTicker24HrAsync(string? symbol = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderBookDepthResponseDto>> GetBinanceSpotDepthAsync(string symbol, CancellationToken cancellationToken = default);
    Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBinanceSpotTicker24HrAsync(string? symbol = null, CancellationToken cancellationToken = default);

    Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBybitLinearTickersAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBybitSpotTickersAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<StockPriceHistoryResponseDto>> GetStockPriceHistoryAsync(string symbol, CancellationToken cancellationToken = default);
    Task<ApiResponse<WatchlistPriceResponseDto>> GetWatchlistPriceAsync(string symbol, CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectEventsResponseDto>> GetVnDirectEventsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectRatiosResponseDto>> GetVnDirectRatiosLatestMarketAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectRatiosResponseDto>> GetVnDirectRatiosLatestAsync(string code, CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectRecommendationsResponseDto>> GetVnDirectRecommendationsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectStockPricesResponseDto>> GetVnDirectStockPricesAsync(string code, CancellationToken cancellationToken = default);
    Task<ApiResponse<VnDirectTechnicalSignalsResponseDto>> GetVnDirectTechnicalSignalsAsync(string? strategy = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<TwentyFourHMoneyTransactionsResponseDto>> GetTwentyFourHMoneyTransactionsAsync(
        string symbol,
        int page = 1,
        int perPage = 50,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<Power655ResultsResponseDto>> GetExternalLotteryPower655Async(
        string? dateFrom = null,
        string? dateTo = null,
        CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ChainBrokerFundDto>> GetChainBrokerFundsAsync(CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ChainBrokerProjectDto>> GetChainBrokerProjectsAsync(CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ChainBrokerUnlockDto>> GetChainBrokerUnlocksAsync(CancellationToken cancellationToken = default);
    Task TriggerChainBrokerSyncAsync(CancellationToken cancellationToken = default);
    Task TriggerChainBrokerFundsSyncAsync(CancellationToken cancellationToken = default);
    Task TriggerChainBrokerProjectsSyncAsync(CancellationToken cancellationToken = default);
    Task TriggerChainBrokerUnlocksSyncAsync(CancellationToken cancellationToken = default);
    Task<TcbsTop10PortfoliosResult> GetTcbsTop10PortfoliosAsync(CancellationToken cancellationToken = default);
    Task TriggerTcbsTop10SyncAsync(CancellationToken cancellationToken = default);
    Task<DragonCapitalFundPortfolioDto> GetDragonCapitalFundPortfolioAsync(string fundCode, CancellationToken cancellationToken = default);
    Task<TradeSuggestionDto> GetTradeSuggestionAsync(GetTradeSuggestionRequest request, CancellationToken cancellationToken = default);
    Task<TradeVerdictDto> GetTradeVerdictAsync(GetTradeVerdictRequest request, CancellationToken cancellationToken = default);
    Task<WeeklySuggestionHistoryResultDto> GetWeeklySuggestionHistoryAsync(
        SuggestionAssetClass? assetClass = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<EvaluateWeeklySuggestionPerformanceResultDto> EvaluateWeeklySuggestionHistoryAsync(
        EvaluateWeeklySuggestionPerformanceRequestDto request,
        CancellationToken cancellationToken = default);
    Task<WeeklySuggestionTemplateFileDto> ExportWeeklySuggestionTemplateAsync(
        string reportKey,
        SuggestionAssetClass assetClass,
        string format = "csv",
        CancellationToken cancellationToken = default);
    Task<ApiResponse<TwentyFourHMoneyTransactionHistoryResponseDto>> GetTwentyFourHMoneyTransactionHistoryAsync(
        string symbol,
        decimal? currentPrice = null,
        int page = 1,
        int perPage = 50,
        CancellationToken cancellationToken = default);
    Task TriggerTopStocksWeeklySuggestionJobAsync(CancellationToken cancellationToken = default);
    Task TriggerMexcSpotWeeklySuggestionJobAsync(CancellationToken cancellationToken = default);
}
