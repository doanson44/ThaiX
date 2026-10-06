using ThaiX.Application.Common.Enums;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.Lottery.Queries.GetPower655Results;
using ThaiX.Infrastructure.ExternalApis.Providers;

namespace ThaiX.Infrastructure.Services;

/// <summary>
/// Implementation of IExternalDataService using all API providers.
/// Wraps external API calls and provides them to Application layer via interface.
/// </summary>
public sealed class ExternalDataService : IExternalDataService
{
    private readonly CafeFApiProvider _cafeFApiProvider;
    private readonly BinanceApiProvider _binanceApiProvider;
    private readonly BybitApiProvider _bybitApiProvider;
    private readonly DragonCapitalApiProvider _dragonCapitalApiProvider;
    private readonly SacombankApiProvider _sacombankApiProvider;
    private readonly YahooFinanceApiProvider _yahooFinanceApiProvider;
    private readonly CoinGeckoApiProvider _coinGeckoApiProvider;
    private readonly MexcApiProvider _mexcApiProvider;
    private readonly IWealthClubApiProvider _iWealthClubApiProvider;
    private readonly VnDirectApiProvider _vnDirectApiProvider;
    private readonly TwentyFourHMoneyApiProvider _twentyFourHMoneyApiProvider;
    private readonly ChainBrokerApiProvider _chainBrokerApiProvider;
    private readonly KetQuaDienToanApiProvider _ketQuaDienToanApiProvider;
    private readonly VnExpressApiProvider _vnExpressApiProvider;

    public ExternalDataService(
        CafeFApiProvider cafeFApiProvider,
        BinanceApiProvider binanceApiProvider,
        BybitApiProvider bybitApiProvider,
        DragonCapitalApiProvider dragonCapitalApiProvider,
        SacombankApiProvider sacombankApiProvider,
        YahooFinanceApiProvider yahooFinanceApiProvider,
        CoinGeckoApiProvider coinGeckoApiProvider,
        MexcApiProvider mexcApiProvider,
        IWealthClubApiProvider iWealthClubApiProvider,
        VnDirectApiProvider vnDirectApiProvider,
        TwentyFourHMoneyApiProvider twentyFourHMoneyApiProvider,
        ChainBrokerApiProvider chainBrokerApiProvider,
        KetQuaDienToanApiProvider ketQuaDienToanApiProvider,
        VnExpressApiProvider vnExpressApiProvider)
    {
        _cafeFApiProvider = cafeFApiProvider;
        _binanceApiProvider = binanceApiProvider;
        _bybitApiProvider = bybitApiProvider;
        _dragonCapitalApiProvider = dragonCapitalApiProvider;
        _sacombankApiProvider = sacombankApiProvider;
        _yahooFinanceApiProvider = yahooFinanceApiProvider;
        _coinGeckoApiProvider = coinGeckoApiProvider;
        _mexcApiProvider = mexcApiProvider;
        _iWealthClubApiProvider = iWealthClubApiProvider;
        _vnDirectApiProvider = vnDirectApiProvider;
        _twentyFourHMoneyApiProvider = twentyFourHMoneyApiProvider;
        _chainBrokerApiProvider = chainBrokerApiProvider;
        _ketQuaDienToanApiProvider = ketQuaDienToanApiProvider;
        _vnExpressApiProvider = vnExpressApiProvider;
    }

    public Task<T?> GetBankInterestRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _cafeFApiProvider.GetBankInterestRatesAsync<T>(cancellationToken);
    }

    public Task<T?> GetCommoditiesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _cafeFApiProvider.GetCommoditiesAsync<T>(cancellationToken);
    }

    public Task<T?> GetCurrenciesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _cafeFApiProvider.GetCurrenciesAsync<T>(cancellationToken);
    }

    public Task<T?> GetCryptocurrenciesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _cafeFApiProvider.GetCryptocurrenciesAsync<T>(cancellationToken);
    }

    public Task<T?> GetStockPriceHistoryAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _cafeFApiProvider.GetStockPriceHistoryAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetStockWatchlistPriceAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _cafeFApiProvider.GetStockWatchlistPriceAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetBybitLinearTickersAsync<T>(CancellationToken cancellationToken = default)
    {
        return _bybitApiProvider.GetLinearTickersAsync<T>(cancellationToken);
    }

    public Task<T?> GetBybitSpotTickersAsync<T>(CancellationToken cancellationToken = default)
    {
        return _bybitApiProvider.GetSpotTickersAsync<T>(cancellationToken);
    }

    public Task<T?> GetBinanceFundingRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _binanceApiProvider.GetFundingRatesAsync<T>(cancellationToken);
    }

    public Task<T?> GetBinanceFuturesTicker24HrAsync<T>(CancellationToken cancellationToken = default)
    {
        return _binanceApiProvider.GetFuturesTicker24HrAsync<T>(cancellationToken);
    }

    public Task<T?> GetBinanceFuturesTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _binanceApiProvider.GetFuturesTicker24HrBySymbolAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetBinanceSpotDepthAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _binanceApiProvider.GetSpotDepthAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetBinanceFuturesDepthAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _binanceApiProvider.GetFuturesDepthAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetBinanceSpotTicker24HrAsync<T>(CancellationToken cancellationToken = default)
    {
        return _binanceApiProvider.GetSpotTicker24HrAsync<T>(cancellationToken);
    }

    public Task<T?> GetBinanceSpotTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _binanceApiProvider.GetSpotTicker24HrBySymbolAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetDragonCapitalFundPortfolioAsync<T>(string fundCode, CancellationToken cancellationToken = default)
    {
        return _dragonCapitalApiProvider.GetFundPortfolioAsync<T>(fundCode, cancellationToken);
    }

    public Task<T?> GetSacombankExchangeRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _sacombankApiProvider.GetExchangeRatesAsync<T>(cancellationToken);
    }

    public Task<T?> GetYahooFinanceVixChartAsync<T>(CancellationToken cancellationToken = default)
    {
        return _yahooFinanceApiProvider.GetVixChartAsync<T>(cancellationToken);
    }

    public Task<T?> GetCoinGeckoCoinsListAsync<T>(CancellationToken cancellationToken = default)
    {
        return _coinGeckoApiProvider.GetCoinsListAsync<T>(cancellationToken);
    }

    public Task<T?> GetCoinGeckoCoinMarketByIdAsync<T>(string coinId, CancellationToken cancellationToken = default)
    {
        return _coinGeckoApiProvider.GetCoinMarketByIdAsync<T>(coinId, cancellationToken);
    }

    public Task<T?> GetMexcContractTickerAsync<T>(CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetContractTickerAsync<T>(false, cancellationToken);
    }

    public Task<T?> GetMexcContractTickerAsync<T>(bool bypassCache, CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetContractTickerAsync<T>(bypassCache, cancellationToken);
    }

    public Task<T?> GetMexcContractTickerBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetContractTickerBySymbolAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetMexcContractKlineAsync<T>(string symbol, string? interval = null, CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetContractKlineAsync<T>(symbol, interval, cancellationToken);
    }

    public Task<T?> GetMexcContractFundingRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetContractFundingRatesAsync<T>(cancellationToken);
    }

    public Task<T?> GetMexcContractDepthAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetContractDepthAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetMexcSpotTicker24HrAsync<T>(CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetSpotTicker24HrAsync<T>(cancellationToken);
    }

    public Task<T?> GetMexcSpotTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetSpotTicker24HrBySymbolAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetMexcSpotKlinesAsync<T>(string symbol, string interval, CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetSpotKlinesAsync<T>(symbol, interval, cancellationToken);
    }

    public Task<T?> GetMexcSpotDepthAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        return _mexcApiProvider.GetSpotDepthAsync<T>(symbol, cancellationToken);
    }

    public Task<T?> GetIWealthClubStreamAsync<T>(string? lastContentId = null, CancellationToken cancellationToken = default)
    {
        return _iWealthClubApiProvider.GetStreamAsync<T>(lastContentId, cancellationToken);
    }

    public Task<T?> GetVnDirectTopStocksAsync<T>(CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetTopStocksAsync<T>(cancellationToken);
    }

    public Task<T?> GetVnDirectRatiosLatestAsync<T>(string code, CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetRatiosLatestAsync<T>(code, cancellationToken);
    }

    public Task<T?> GetVnDirectEventsAsync<T>(CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetEventsAsync<T>(cancellationToken);
    }

    public Task<T?> GetVnDirectStockPricesAsync<T>(string code, CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetStockPricesAsync<T>(code, cancellationToken);
    }

    public Task<T?> GetVnDirectStockPricesAllAsync<T>(CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetStockPricesAllAsync<T>(cancellationToken);
    }

    public Task<T?> GetVnDirectRatiosLatestByItemCodeAsync<T>(CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetRatiosLatestByItemCodeAsync<T>(cancellationToken);
    }

    public Task<T?> GetVnDirectRecommendationsAsync<T>(CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetRecommendationsAsync<T>(cancellationToken);
    }

    public Task<T?> GetVnDirectTechnicalSignalsAsync<T>(string strategy, CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetTechnicalSignalsAsync<T>(strategy, cancellationToken);
    }

    public Task<T?> GetVnDirectChangePricesAsync<T>(CancellationToken cancellationToken = default)
    {
        return _vnDirectApiProvider.GetChangePricesAsync<T>(cancellationToken);
    }

    public Task<T?> GetTwentyFourHMoneyTransactionListSsiAsync<T>(
        string symbol,
        int page = 1,
        int perPage = 1000,
        CancellationToken cancellationToken = default)
    {
        return _twentyFourHMoneyApiProvider.GetTransactionListSsiAsync<T>(symbol, page, perPage, cancellationToken);
    }

    public Task<T?> GetChainBrokerUnlocksListAsync<T>(int? page = null, CancellationToken cancellationToken = default)
    {
        return _chainBrokerApiProvider.GetUnlocksListAsync<T>(page, cancellationToken);
    }

    public Task<T?> GetChainBrokerProjectsListAsync<T>(int? page = null, CancellationToken cancellationToken = default)
    {
        return _chainBrokerApiProvider.GetProjectsListAsync<T>(page, cancellationToken);
    }

    public Task<T?> GetChainBrokerFundsListAsync<T>(int? page = null, CancellationToken cancellationToken = default)
    {
        return _chainBrokerApiProvider.GetFundsListAsync<T>(page, cancellationToken);
    }

    public Task<IReadOnlyList<Power655ResultItem>?> GetPower655ResultsAsync(
        string dateFrom,
        string dateTo,
        CancellationToken cancellationToken = default)
    {
        return _ketQuaDienToanApiProvider.GetPower655ResultsAsync(dateFrom, dateTo, cancellationToken);
    }

    public Task<T?> GetVnExpressBankRatesAsync<T>(BankRateChannel channel, CancellationToken cancellationToken = default)
    {
        return _vnExpressApiProvider.GetBankRateAsync<T>(channel, cancellationToken);
    }
}
