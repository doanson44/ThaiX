using ThaiX.Application.Common.Enums;

namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Interface for retrieving external data from third-party APIs.
/// Implementation resides in Infrastructure layer with actual HTTP calls.
/// </summary>
public interface IExternalDataService
{
    /// <summary>
    /// Gets current bank interest rates from CafeF external API.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBankInterestRatesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets bank deposit interest rates of the requested channel (online or counter) from VnExpress external API.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnExpressBankRatesAsync<T>(BankRateChannel channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current commodity prices (gold, silver, oil, metals, agricultural products) from CafeF external API.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetCommoditiesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current currency exchange rates (USD, EUR, GBP, etc.) from CafeF external API.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetCurrenciesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current cryptocurrency prices (Bitcoin, Ethereum, etc.) from CafeF external API.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetCryptocurrenciesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stock price history from CafeF external API by stock symbol (e.g. VNM, VIC, NKG).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetStockPriceHistoryAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current watchlist price snapshot from CafeF by stock symbol (e.g. VNM, VIC, NKG).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetStockWatchlistPriceAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Bybit linear/perpetual market tickers.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBybitLinearTickersAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Bybit spot market tickers.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBybitSpotTickersAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Binance futures funding rate history.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBinanceFundingRatesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Binance futures 24hr ticker data for all symbols.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBinanceFuturesTicker24HrAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Binance futures 24hr ticker data by symbol.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBinanceFuturesTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Binance spot order book depth by symbol (limit fixed in configuration).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBinanceSpotDepthAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Binance futures order book depth by symbol (limit fixed in configuration).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBinanceFuturesDepthAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Binance spot 24hr ticker data for all symbols.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBinanceSpotTicker24HrAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Binance spot 24hr ticker data by symbol.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetBinanceSpotTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Dragon Capital fund portfolio data by fund code.
    /// Supported fund codes: VF1, VF4, VFMVN30, VFMVND, VFMMID.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetDragonCapitalFundPortfolioAsync<T>(string fundCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Sacombank exchange rates for currencies and gold.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetSacombankExchangeRatesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Yahoo Finance VIX (Volatility Index) chart data.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetYahooFinanceVixChartAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets CoinGecko coins list (id, symbol, name).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetCoinGeckoCoinsListAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets CoinGecko coin market data by coin id (e.g. bitcoin, ethereum).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetCoinGeckoCoinMarketByIdAsync<T>(string coinId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC contract ticker for all market symbols.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcContractTickerAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC contract ticker for all market symbols.
    /// When bypassCache is true, forces a direct request to the external provider, skipping the transport cache.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcContractTickerAsync<T>(bool bypassCache, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC contract ticker by symbol (example: BTC_USDT).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcContractTickerBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC contract kline data by symbol with optional interval.
    /// Interval defaults to Min1 when omitted.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcContractKlineAsync<T>(string symbol, string? interval = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC contract funding rates for all symbols.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcContractFundingRatesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC contract depth by symbol (example: BTC_USDT).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcContractDepthAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC spot 24h ticker statistics for all symbols.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcSpotTicker24HrAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC spot 24h ticker statistics by symbol (example: BTCUSDT).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcSpotTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC spot klines by symbol and interval.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcSpotKlinesAsync<T>(string symbol, string interval, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MEXC spot depth by symbol.
    /// Limit is fixed by provider configuration.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetMexcSpotDepthAsync<T>(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets iWealth Club stream content by optional lastContentId cursor.
    /// First call should omit cursor (null).
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetIWealthClubStreamAsync<T>(string? lastContentId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect top stocks list.
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectTopStocksAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect latest ratios by stock code.
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectRatiosLatestAsync<T>(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect stock events list (nomargin, alert, halt, control, suspend, noticed).
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectEventsAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect stock price history by stock code.
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectStockPricesAsync<T>(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect stock prices for the entire market (no code filter, most recent records).
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectStockPricesAllAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect latest market ratios by predefined item codes.
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectRatiosLatestByItemCodeAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect recommendations list.
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectRecommendationsAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect technical signals list by strategy.
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectTechnicalSignalsAsync<T>(string strategy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets VnDirect change prices for market indices (VNINDEX, HNX, UPCOM, VN30, VN30F1M) for period 1D.
    /// This endpoint requires proxy routing.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetVnDirectChangePricesAsync<T>(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets 24HMoney SSI transaction list by stock symbol.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetTwentyFourHMoneyTransactionListSsiAsync<T>(
        string symbol,
        int page = 1,
        int perPage = 1000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ChainBroker token unlocks list. Supports optional page parameter for pagination.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetChainBrokerUnlocksListAsync<T>(int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ChainBroker projects list. Supports optional page parameter for pagination.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetChainBrokerProjectsListAsync<T>(int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ChainBroker funds list. Supports optional page parameter for pagination.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<T?> GetChainBrokerFundsListAsync<T>(int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Power 6/55 lottery results from ketquadientoan.com.
    /// Date parameters use dd-MM-yyyy format.
    /// Returns null if the external API call fails or returns no data.
    /// </summary>
    Task<IReadOnlyList<Features.ExternalData.Lottery.Queries.GetPower655Results.Power655ResultItem>?> GetPower655ResultsAsync(
        string dateFrom,
        string dateTo,
        CancellationToken cancellationToken = default);
}
