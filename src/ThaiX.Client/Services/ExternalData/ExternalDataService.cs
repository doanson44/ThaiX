using System.Net;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.ExternalData;

/// <summary>
/// HTTP client implementation for external data API.
/// </summary>
public sealed class ExternalDataService : IExternalDataService
{
    private const string ExternalDataBase = "api/external-data";
    private const string BankInterestRatesEndpoint = "bank-interest-rates";
    private const string BankDepositRatesEndpoint = "bank-deposit-rates";
    private const string SacombankExchangeRatesEndpoint = "sacombank/exchange-rates";
    private const string MarketCommoditiesEndpoint = "market/commodities";
    private const string MarketCurrenciesEndpoint = "market/currencies";
    private const string MarketCryptocurrenciesEndpoint = "market/cryptocurrencies";
    private const string MarketCoinGeckoCoinsListEndpoint = "market/coingecko/coins-list";
    private const string MarketCoinGeckoByIdEndpoint = "market/coingecko/markets";
    private const string MarketYahooFinanceVixEndpoint = "yahoo-finance/vix";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public ExternalDataService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<ApiResponse<BankInterestRatesResponseDto>> GetBankInterestRatesAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ExternalBankRates,
            BankInterestRatesEndpoint,
            FetchBankInterestRatesAsync,
            ClientCacheTtl.BankRates,
            cancellationToken);
    }

    public Task<ApiResponse<SacombankExchangeRatesResponseDto>> GetSacombankExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ExternalBankRates,
            SacombankExchangeRatesEndpoint,
            FetchSacombankExchangeRatesAsync,
            ClientCacheTtl.BankRates,
            cancellationToken);
    }

    /// <summary>Loads one channel ("online" or "offline") of the VnExpress bank deposit rates.</summary>
    public Task<ApiResponse<BankDepositRatesResponseDto>> GetBankDepositRatesAsync(string type, CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<BankDepositRatesResponseDto>(
            CacheGroups.ExternalBankRates,
            $"{BankDepositRatesEndpoint}?type={type}",
            ClientCacheTtl.BankRates,
            cancellationToken);

    public Task<ApiResponse<CommoditiesResponseDto>> GetCommoditiesAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<CommoditiesResponseDto>(
            CacheGroups.ExternalMarket,
            MarketCommoditiesEndpoint,
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<CurrenciesResponseDto>> GetCurrenciesAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<CurrenciesResponseDto>(
            CacheGroups.ExternalMarket,
            MarketCurrenciesEndpoint,
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<CryptocurrenciesResponseDto>> GetCryptocurrenciesAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<CryptocurrenciesResponseDto>(
            CacheGroups.ExternalMarket,
            MarketCryptocurrenciesEndpoint,
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<CoinGeckoCoinsListResponseDto>> GetCoinGeckoCoinsListAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<CoinGeckoCoinsListResponseDto>(
            CacheGroups.CoinGeckoCoinsList,
            MarketCoinGeckoCoinsListEndpoint,
            ClientCacheTtl.CoinGeckoList,
            cancellationToken);

    public Task<ApiResponse<CoinGeckoCoinMarketResponseDto>> GetCoinGeckoCoinMarketByIdAsync(
        string coinId,
        CancellationToken cancellationToken = default)
    {
        var relativePath = $"{MarketCoinGeckoByIdEndpoint}/{Uri.EscapeDataString(coinId)}";
        return GetEnvelopeCachedAsync<CoinGeckoCoinMarketResponseDto>(
            CacheGroups.ExternalMarket,
            relativePath,
            ClientCacheTtl.Ticker,
            cancellationToken);
    }

    public Task<ApiResponse<VixResponseDto>> GetYahooFinanceVixAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VixResponseDto>(
            CacheGroups.ExternalMarket,
            MarketYahooFinanceVixEndpoint,
            ClientCacheTtl.Funding,
            cancellationToken);

    public async Task<VixVerdictDto> GetVixVerdictAsync(
        GetVixVerdictRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = $"{ExternalDataBase}/{MarketYahooFinanceVixEndpoint}/verdict";
        using var response = await _httpClient.PostAsJsonAsync(path, request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<VixVerdictDto>(response, cancellationToken);
    }

    public Task<ApiResponse<VnDirectChangePricesResponseDto>> GetVnDirectChangePricesAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VnDirectChangePricesResponseDto>(
            CacheGroups.ExternalVnDirect,
            "market/vndirect/change-prices",
            ClientCacheTtl.VnDirect,
            cancellationToken);

    public Task<ApiResponse<VnDirectTopStocksResponseDto>> GetVnDirectTopStocksAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VnDirectTopStocksResponseDto>(
            CacheGroups.ExternalVnDirect,
            "market/vndirect/top-stocks",
            ClientCacheTtl.VnDirect,
            cancellationToken);

    public Task<ApiResponse<MexcContractTickersResponseDto>> GetMexcContractTickersAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<MexcContractTickersResponseDto>(
            CacheGroups.ExternalExchangeMexc,
            "mexc/contract-ticker",
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<MexcSpotTicker24HrResponseDto>> GetMexcSpotTicker24HrAsync(CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<MexcSpotTicker24HrResponseDto>(
            CacheGroups.ExternalExchangeMexc,
            "mexc/spot-ticker-24hr",
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<OrderBookDepthResponseDto>> GetMexcContractDepthAsync(
        string symbol,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<OrderBookDepthResponseDto>(
            CacheGroups.ExternalExchangeMexc,
            $"mexc/contract-depth/{Uri.EscapeDataString(symbol)}",
            ClientCacheTtl.Depth,
            cancellationToken);

    public Task<ApiResponse<KlineSeriesResponseDto>> GetMexcContractKlineAsync(
        string symbol,
        string? interval = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"mexc/contract-kline/{Uri.EscapeDataString(symbol)}";
        if (!string.IsNullOrWhiteSpace(interval))
            path += $"?interval={Uri.EscapeDataString(interval)}";
        return GetEnvelopeCachedAsync<KlineSeriesResponseDto>(
            CacheGroups.ExternalExchangeMexc,
            path,
            ClientCacheTtl.Ticker,
            cancellationToken);
    }

    public Task<ApiResponse<FundingRatesResponseDto>> GetMexcFundingRatesAsync(
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<FundingRatesResponseDto>(
            CacheGroups.ExternalExchangeMexc,
            "mexc/funding-rate",
            ClientCacheTtl.Funding,
            cancellationToken);

    public Task<ApiResponse<OrderBookDepthResponseDto>> GetMexcSpotDepthAsync(
        string symbol,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<OrderBookDepthResponseDto>(
            CacheGroups.ExternalExchangeMexc,
            $"mexc/spot-depth/{Uri.EscapeDataString(symbol)}",
            ClientCacheTtl.Depth,
            cancellationToken);

    public Task<ApiResponse<KlineSeriesResponseDto>> GetMexcSpotKlinesAsync(
        string symbol,
        string? interval = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"mexc/spot-klines/{Uri.EscapeDataString(symbol)}";
        if (!string.IsNullOrWhiteSpace(interval))
            path += $"?interval={Uri.EscapeDataString(interval)}";
        return GetEnvelopeCachedAsync<KlineSeriesResponseDto>(
            CacheGroups.ExternalExchangeMexc,
            path,
            ClientCacheTtl.Ticker,
            cancellationToken);
    }

    public Task<ApiResponse<FundingRatesResponseDto>> GetBinanceFundingRatesAsync(
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<FundingRatesResponseDto>(
            CacheGroups.ExternalExchangeBinance,
            "binance/funding-rates",
            ClientCacheTtl.Funding,
            cancellationToken);

    public Task<ApiResponse<OrderBookDepthResponseDto>> GetBinanceFuturesDepthAsync(
        string symbol,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<OrderBookDepthResponseDto>(
            CacheGroups.ExternalExchangeBinance,
            $"binance/futures-depth/{Uri.EscapeDataString(symbol)}",
            ClientCacheTtl.Depth,
            cancellationToken);

    public Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBinanceFuturesTicker24HrAsync(
        string? symbol = null,
        CancellationToken cancellationToken = default)
    {
        var path = "binance/futures-ticker-24hr";
        if (!string.IsNullOrWhiteSpace(symbol))
            path += $"?symbol={Uri.EscapeDataString(symbol)}";
        return GetEnvelopeCachedAsync<ExchangeTicker24HrResponseDto>(
            CacheGroups.ExternalExchangeBinance,
            path,
            ClientCacheTtl.Ticker,
            cancellationToken);
    }

    public Task<ApiResponse<OrderBookDepthResponseDto>> GetBinanceSpotDepthAsync(
        string symbol,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<OrderBookDepthResponseDto>(
            CacheGroups.ExternalExchangeBinance,
            $"binance/spot-depth/{Uri.EscapeDataString(symbol)}",
            ClientCacheTtl.Depth,
            cancellationToken);

    public Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBinanceSpotTicker24HrAsync(
        string? symbol = null,
        CancellationToken cancellationToken = default)
    {
        var path = "binance/spot-ticker-24hr";
        if (!string.IsNullOrWhiteSpace(symbol))
            path += $"?symbol={Uri.EscapeDataString(symbol)}";
        return GetEnvelopeCachedAsync<ExchangeTicker24HrResponseDto>(
            CacheGroups.ExternalExchangeBinance,
            path,
            ClientCacheTtl.Ticker,
            cancellationToken);
    }

    public Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBybitLinearTickersAsync(
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<ExchangeTicker24HrResponseDto>(
            CacheGroups.ExternalExchangeBybit,
            "bybit/linear-tickers",
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<ExchangeTicker24HrResponseDto>> GetBybitSpotTickersAsync(
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<ExchangeTicker24HrResponseDto>(
            CacheGroups.ExternalExchangeBybit,
            "bybit/spot-tickers",
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<StockPriceHistoryResponseDto>> GetStockPriceHistoryAsync(
        string symbol,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<StockPriceHistoryResponseDto>(
            CacheGroups.ExternalMarket,
            $"market/price-history/{Uri.EscapeDataString(symbol)}",
            ClientCacheTtl.StockHistory,
            cancellationToken);

    public Task<ApiResponse<WatchlistPriceResponseDto>> GetWatchlistPriceAsync(
        string symbol,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<WatchlistPriceResponseDto>(
            CacheGroups.ExternalMarket,
            $"market/watchlist-price/{Uri.EscapeDataString(symbol)}",
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<VnDirectEventsResponseDto>> GetVnDirectEventsAsync(
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VnDirectEventsResponseDto>(
            CacheGroups.ExternalVnDirect,
            "market/vndirect/events",
            ClientCacheTtl.VnDirect,
            cancellationToken);

    public Task<ApiResponse<VnDirectRatiosResponseDto>> GetVnDirectRatiosLatestMarketAsync(
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VnDirectRatiosResponseDto>(
            CacheGroups.ExternalVnDirect,
            "market/vndirect/ratios/latest/market",
            ClientCacheTtl.VnDirect,
            cancellationToken);

    public Task<ApiResponse<VnDirectRatiosResponseDto>> GetVnDirectRatiosLatestAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VnDirectRatiosResponseDto>(
            CacheGroups.ExternalVnDirect,
            $"market/vndirect/ratios/latest/{Uri.EscapeDataString(code)}",
            ClientCacheTtl.VnDirect,
            cancellationToken);

    public Task<ApiResponse<VnDirectRecommendationsResponseDto>> GetVnDirectRecommendationsAsync(
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VnDirectRecommendationsResponseDto>(
            CacheGroups.ExternalVnDirect,
            "market/vndirect/recommendations",
            ClientCacheTtl.VnDirect,
            cancellationToken);

    public Task<ApiResponse<VnDirectStockPricesResponseDto>> GetVnDirectStockPricesAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<VnDirectStockPricesResponseDto>(
            CacheGroups.ExternalVnDirect,
            $"market/vndirect/stock-prices/{Uri.EscapeDataString(code)}",
            ClientCacheTtl.VnDirect,
            cancellationToken);

    public Task<ApiResponse<VnDirectTechnicalSignalsResponseDto>> GetVnDirectTechnicalSignalsAsync(
        string? strategy = null,
        CancellationToken cancellationToken = default)
    {
        var path = "market/vndirect/technical-signals";
        if (!string.IsNullOrWhiteSpace(strategy))
            path += $"?strategy={Uri.EscapeDataString(strategy)}";
        return GetEnvelopeCachedAsync<VnDirectTechnicalSignalsResponseDto>(
            CacheGroups.ExternalVnDirect,
            path,
            ClientCacheTtl.VnDirect,
            cancellationToken);
    }

    public Task<ApiResponse<TwentyFourHMoneyTransactionsResponseDto>> GetTwentyFourHMoneyTransactionsAsync(
        string symbol,
        int page = 1,
        int perPage = 50,
        CancellationToken cancellationToken = default) =>
        GetEnvelopeCachedAsync<TwentyFourHMoneyTransactionsResponseDto>(
            CacheGroups.ExternalMarket,
            $"market/24hmoney/transactions/{Uri.EscapeDataString(symbol)}?page={page}&perPage={perPage}",
            ClientCacheTtl.Ticker,
            cancellationToken);

    public Task<ApiResponse<Power655ResultsResponseDto>> GetExternalLotteryPower655Async(
        string? dateFrom = null,
        string? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var path = "lottery/power-655";
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(dateFrom))
            qs.Add($"dateFrom={Uri.EscapeDataString(dateFrom)}");
        if (!string.IsNullOrWhiteSpace(dateTo))
            qs.Add($"dateTo={Uri.EscapeDataString(dateTo)}");
        if (qs.Count > 0)
            path += "?" + string.Join("&", qs);
        return GetEnvelopeCachedAsync<Power655ResultsResponseDto>(
            CacheGroups.ExternalLotteryPower655,
            path,
            cancellationToken: cancellationToken);
    }

    public Task<PagedApiResponse<ChainBrokerFundDto>> GetChainBrokerFundsAsync(CancellationToken cancellationToken = default)
    {
        const string RelativePath = "chainbroker/db/funds?pageSize=-1";
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ChainBrokerFunds,
            RelativePath,
            async ct =>
            {
                var path = $"{ExternalDataBase}/{RelativePath}";
                var response = await _httpClient.GetAsync(path, ct);
                return await ApiResponseReader.ReadPagedSuccessAsync<ChainBrokerFundDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public Task<PagedApiResponse<ChainBrokerProjectDto>> GetChainBrokerProjectsAsync(CancellationToken cancellationToken = default)
    {
        const string RelativePath = "chainbroker/db/projects?pageSize=-1";
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ChainBrokerProjects,
            RelativePath,
            async ct =>
            {
                var path = $"{ExternalDataBase}/{RelativePath}";
                var response = await _httpClient.GetAsync(path, ct);
                return await ApiResponseReader.ReadPagedSuccessAsync<ChainBrokerProjectDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public Task<PagedApiResponse<ChainBrokerUnlockDto>> GetChainBrokerUnlocksAsync(CancellationToken cancellationToken = default)
    {
        const string RelativePath = "chainbroker/db/unlocks?pageSize=-1";
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ChainBrokerUnlocks,
            RelativePath,
            async ct =>
            {
                var path = $"{ExternalDataBase}/{RelativePath}";
                var response = await _httpClient.GetAsync(path, ct);
                return await ApiResponseReader.ReadPagedSuccessAsync<ChainBrokerUnlockDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task TriggerChainBrokerSyncAsync(CancellationToken cancellationToken = default)
    {
        const string Endpoint = "chainbroker/sync";
        var path = $"{ExternalDataBase}/{Endpoint}";
        var response = await _httpClient.PostAsync(path, null, cancellationToken);
        await ApiResponseReader.ReadSuccessDataAsync<object>(response, cancellationToken);
        await InvalidateChainBrokerCachesAsync(cancellationToken);
    }

    public async Task TriggerChainBrokerFundsSyncAsync(CancellationToken cancellationToken = default)
    {
        const string Endpoint = "chainbroker/sync/funds";
        var path = $"{ExternalDataBase}/{Endpoint}";
        var response = await _httpClient.PostAsync(path, null, cancellationToken);
        await ApiResponseReader.ReadSuccessDataAsync<object>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerFunds, cancellationToken);
    }

    public async Task TriggerChainBrokerProjectsSyncAsync(CancellationToken cancellationToken = default)
    {
        const string Endpoint = "chainbroker/sync/projects";
        var path = $"{ExternalDataBase}/{Endpoint}";
        var response = await _httpClient.PostAsync(path, null, cancellationToken);
        await ApiResponseReader.ReadSuccessDataAsync<object>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerProjects, cancellationToken);
    }

    public async Task TriggerChainBrokerUnlocksSyncAsync(CancellationToken cancellationToken = default)
    {
        const string Endpoint = "chainbroker/sync/unlocks";
        var path = $"{ExternalDataBase}/{Endpoint}";
        var response = await _httpClient.PostAsync(path, null, cancellationToken);
        await ApiResponseReader.ReadSuccessDataAsync<object>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerUnlocks, cancellationToken);
    }

    public Task<TcbsTop10PortfoliosResult> GetTcbsTop10PortfoliosAsync(CancellationToken cancellationToken = default)
    {
        const string Endpoint = "tcbs-top10/portfolios";
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.TcbsTop10,
            Endpoint,
            async ct =>
            {
                var path = $"{ExternalDataBase}/{Endpoint}";
                var response = await _httpClient.GetAsync(path, ct);
                return await ApiResponseReader.ReadSuccessDataAsync<TcbsTop10PortfoliosResult>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task TriggerTcbsTop10SyncAsync(CancellationToken cancellationToken = default)
    {
        const string Endpoint = "tcbs-top10/sync";
        var path = $"{ExternalDataBase}/{Endpoint}";
        var response = await _httpClient.PostAsync(path, null, cancellationToken);
        await ApiResponseReader.ReadSuccessDataAsync<object>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.TcbsTop10, cancellationToken);
    }

    public Task<DragonCapitalFundPortfolioDto> GetDragonCapitalFundPortfolioAsync(
        string fundCode,
        CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.DragonCapitalPortfolio,
            fundCode,
            async ct =>
            {
                var path = $"{ExternalDataBase}/dragon-capital/fund-portfolio/{fundCode}";
                var response = await _httpClient.GetAsync(path, ct);
                return await ApiResponseReader.ReadSuccessDataAsync<DragonCapitalFundPortfolioDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public Task<TradeSuggestionDto> GetTradeSuggestionAsync(
        GetTradeSuggestionRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/trading/suggestion" +
                  $"?symbol={Uri.EscapeDataString(request.Symbol)}" +
                  $"&marketType={(int)request.MarketType}" +
                  $"&timeframe={Uri.EscapeDataString(request.Timeframe)}" +
                  $"&useLongTermTimeframe={(request.UseLongTermTimeframe ? "true" : "false")}" +
                  $"&marketRegime={(int)request.MarketRegime}" +
                  $"&eventRisk={(int)request.EventRisk}";

        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ExternalTradeSuggestion,
            url,
            async ct =>
            {
                using var response = await _httpClient.GetAsync(url, ct);
                return await ApiResponseReader.ReadSuccessDataAsync<TradeSuggestionDto>(response, ct);
            },
            ClientCacheTtl.TradeSuggestion,
            cancellationToken);
    }

    public async Task<TradeVerdictDto> GetTradeVerdictAsync(
        GetTradeVerdictRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/trading/verdict", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<TradeVerdictDto>(response, cancellationToken);
    }

    public Task<WeeklySuggestionHistoryResultDto> GetWeeklySuggestionHistoryAsync(
        SuggestionAssetClass? assetClass = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/trading/suggestion-history?page={page}&pageSize={pageSize}";
        if (assetClass.HasValue)
        {
            url += $"&assetClass={(int)assetClass.Value}";
        }

        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.WeeklySuggestionHistory,
            url,
            async ct =>
            {
                using var response = await _httpClient.GetAsync(url, ct);
                return await ApiResponseReader.ReadSuccessDataAsync<WeeklySuggestionHistoryResultDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<EvaluateWeeklySuggestionPerformanceResultDto> EvaluateWeeklySuggestionHistoryAsync(
        EvaluateWeeklySuggestionPerformanceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/trading/suggestion-history/evaluate", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<EvaluateWeeklySuggestionPerformanceResultDto>(response, cancellationToken);
    }

    public async Task<WeeklySuggestionTemplateFileDto> ExportWeeklySuggestionTemplateAsync(
        string reportKey,
        SuggestionAssetClass assetClass,
        string format = "csv",
        CancellationToken cancellationToken = default)
    {
        var url = $"api/trading/suggestion-history/export" +
                  $"?reportKey={Uri.EscapeDataString(reportKey)}" +
                  $"&assetClass={(int)assetClass}" +
                  $"&format={Uri.EscapeDataString(format)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
            throw new ApiException(
                code: "CLIENT_EXPORT_FAILED",
                message: $"Template export request failed with status {(int)response.StatusCode}.",
                statusCode: response.StatusCode);
        }

        var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? $"weekly_suggestion_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{(format.Equals("csv", StringComparison.OrdinalIgnoreCase) ? "csv" : "md")}";
        var contentType = response.Content.Headers.ContentType?.ToString()
            ?? (format.Equals("csv", StringComparison.OrdinalIgnoreCase) ? "text/csv" : "text/markdown");

        return new WeeklySuggestionTemplateFileDto
        {
            FileName = fileName,
            ContentType = contentType,
            FileContent = fileContent
        };
    }

    public Task<ApiResponse<TwentyFourHMoneyTransactionHistoryResponseDto>> GetTwentyFourHMoneyTransactionHistoryAsync(
        string symbol,
        decimal? currentPrice = null,
        int page = 1,
        int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var relativePath = $"market/24hmoney/transactions/{Uri.EscapeDataString(symbol)}/history" +
                           $"?page={page}&perPage={perPage}";
        if (currentPrice.HasValue)
        {
            relativePath += $"&currentPrice={currentPrice.Value}";
        }

        return GetEnvelopeCachedAsync<TwentyFourHMoneyTransactionHistoryResponseDto>(
            CacheGroups.ExternalMarket,
            relativePath,
            ClientCacheTtl.Ticker,
            cancellationToken);
    }

    public async Task TriggerTopStocksWeeklySuggestionJobAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("api/trading/weekly-suggestion/run", null, cancellationToken);
        await ApiResponseReader.ReadSuccessDataAsync<object>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.WeeklySuggestionHistory, cancellationToken);
    }

    public async Task TriggerMexcSpotWeeklySuggestionJobAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("api/trading/weekly-suggestion/run-spot", null, cancellationToken);
        await ApiResponseReader.ReadSuccessDataAsync<object>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.WeeklySuggestionHistory, cancellationToken);
    }

    private async Task<ApiResponse<BankInterestRatesResponseDto>> FetchBankInterestRatesAsync(CancellationToken cancellationToken)
    {
        var path = $"{ExternalDataBase}/{BankInterestRatesEndpoint}";
        var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(
                code: "EXTERNAL_API_ERROR",
                message: $"External API call failed with status {response.StatusCode}",
                statusCode: response.StatusCode);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<BankInterestRatesResponseDto>>(
            ApiJsonOptions.Default,
            cancellationToken);

        if (envelope is null)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_RESPONSE",
                message: "Invalid response payload from external API.",
                statusCode: HttpStatusCode.InternalServerError);
        }

        return envelope;
    }

    private async Task<ApiResponse<SacombankExchangeRatesResponseDto>> FetchSacombankExchangeRatesAsync(CancellationToken cancellationToken)
    {
        var path = $"{ExternalDataBase}/{SacombankExchangeRatesEndpoint}";
        var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(
                code: "EXTERNAL_API_ERROR",
                message: $"External API call failed with status {response.StatusCode}",
                statusCode: response.StatusCode);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<SacombankExchangeRatesResponseDto>>(
            ApiJsonOptions.Default,
            cancellationToken);

        if (envelope is null)
        {
            throw new ApiException(
                code: "CLIENT_INVALID_RESPONSE",
                message: "Invalid response payload from external API.",
                statusCode: HttpStatusCode.InternalServerError);
        }

        return envelope;
    }

    private Task<ApiResponse<T>> GetEnvelopeCachedAsync<T>(
        string group,
        string relativePath,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            group,
            relativePath,
            ct => GetEnvelopeAsync<T>(relativePath, ct),
            absoluteExpiration,
            cancellationToken);
    }

    private async Task<ApiResponse<T>> GetEnvelopeAsync<T>(string relativePath, CancellationToken cancellationToken)
    {
        var path = $"{ExternalDataBase}/{relativePath}";
        var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiException(
                "EXTERNAL_API_ERROR",
                $"API call failed: {response.StatusCode}",
                response.StatusCode);
        }

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(
            ApiJsonOptions.Default,
            cancellationToken);
        if (envelope is null)
        {
            throw new ApiException(
                "CLIENT_INVALID_RESPONSE",
                "Invalid response from external API.",
                HttpStatusCode.InternalServerError);
        }

        return envelope;
    }

    private async Task InvalidateChainBrokerCachesAsync(CancellationToken cancellationToken)
    {
        await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerFunds, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerProjects, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.ChainBrokerUnlocks, cancellationToken);
    }
}
