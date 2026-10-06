using System.Collections;
using ThaiX.Application.Common.Enums;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.Lottery.Queries.GetPower655Results;

namespace ThaiX.Application.UnitTests.Common;

internal class StubExternalDataService : IExternalDataService
{
    public bool ShouldReturnNullForMexcSpotTicker24Hr { get; set; }
    public bool ShouldReturnNullForMexcContractTicker { get; set; }

    public virtual Task<T?> GetBankInterestRatesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnExpressBankRatesAsync<T>(BankRateChannel channel, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetCommoditiesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetCurrenciesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetCryptocurrenciesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetStockPriceHistoryAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetStockWatchlistPriceAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBybitLinearTickersAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBybitSpotTickersAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBinanceFundingRatesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBinanceFuturesTicker24HrAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBinanceFuturesTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBinanceSpotDepthAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBinanceFuturesDepthAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBinanceSpotTicker24HrAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetBinanceSpotTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetDragonCapitalFundPortfolioAsync<T>(string fundCode, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetSacombankExchangeRatesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetYahooFinanceVixChartAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetCoinGeckoCoinsListAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetCoinGeckoCoinMarketByIdAsync<T>(string coinId, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetMexcContractTickerAsync<T>(CancellationToken cancellationToken = default)
        => GetMexcContractTickerAsync<T>(false, cancellationToken);

    public virtual Task<T?> GetMexcContractTickerAsync<T>(bool bypassCache, CancellationToken cancellationToken = default)
    {
        if (ShouldReturnNullForMexcContractTicker)
            return Task.FromResult((T?)(object?)null);

        return Task.FromResult((T?)CreateMexcContractTickerResponse(typeof(T)));
    }

    public virtual Task<T?> GetMexcContractTickerBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetMexcContractKlineAsync<T>(string symbol, string? interval = null, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetMexcContractFundingRatesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetMexcContractDepthAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetMexcSpotTicker24HrAsync<T>(CancellationToken cancellationToken = default)
    {
        if (ShouldReturnNullForMexcSpotTicker24Hr)
            return Task.FromResult((T?)(object?)null);

        return Task.FromResult((T?)CreateMexcSpotTicker24HrResponse(typeof(T)));
    }

    public virtual Task<T?> GetMexcSpotTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetMexcSpotKlinesAsync<T>(string symbol, string interval, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetMexcSpotDepthAsync<T>(string symbol, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetIWealthClubStreamAsync<T>(string? lastContentId = null, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectTopStocksAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectRatiosLatestAsync<T>(string code, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectEventsAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectStockPricesAsync<T>(string code, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);

    private static object? CreateMexcSpotTicker24HrResponse(Type resultType)
    {
        if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition() != typeof(List<>))
            return null;

        var itemType = resultType.GetGenericArguments()[0];
        if (itemType.Name != "InternalMexcSpotTicker24Hr")
            return null;

        var listType = typeof(List<>).MakeGenericType(itemType);
        var list = (IList)Activator.CreateInstance(listType)!;
        var item = Activator.CreateInstance(itemType)!;

        itemType.GetProperty("Symbol")?.SetValue(item, "BTCUSDT");
        itemType.GetProperty("QuoteVolume")?.SetValue(item, "1000");
        itemType.GetProperty("PriceChangePercent")?.SetValue(item, "0.02");
        itemType.GetProperty("PriceChange")?.SetValue(item, "20");
        itemType.GetProperty("PrevClosePrice")?.SetValue(item, "50000");
        itemType.GetProperty("LastPrice")?.SetValue(item, "52000");
        itemType.GetProperty("BidPrice")?.SetValue(item, "51900");
        itemType.GetProperty("AskPrice")?.SetValue(item, "52100");
        itemType.GetProperty("OpenTime")?.SetValue(item, 1L);
        itemType.GetProperty("CloseTime")?.SetValue(item, 2L);

        list.Add(item);
        return list;
    }

    private static object? CreateMexcContractTickerResponse(Type resultType)
    {
        if (resultType.Name is not "InternalMexcApiArrayResponse" and not "InternalMexcApiResponse")
            return null;

        var response = Activator.CreateInstance(resultType)!;
        resultType.GetProperty("Success")?.SetValue(response, true);
        resultType.GetProperty("Code")?.SetValue(response, 0);

        var dataProperty = resultType.GetProperty("Data");
        if (dataProperty is null)
            return null;

        var dataType = dataProperty.PropertyType;
        if (!dataType.IsGenericType || dataType.GetGenericTypeDefinition() != typeof(List<>))
            return null;

        var itemType = dataType.GetGenericArguments()[0];
        var list = (IList)Activator.CreateInstance(dataType)!;
        var item = Activator.CreateInstance(itemType)!;

        itemType.GetProperty("ContractId")?.SetValue(item, 1);
        itemType.GetProperty("Symbol")?.SetValue(item, "BTC_USDT");
        itemType.GetProperty("Amount24")?.SetValue(item, 1000m);
        itemType.GetProperty("HoldVol")?.SetValue(item, 500m);
        itemType.GetProperty("FundingRate")?.SetValue(item, 0.0001m);
        itemType.GetProperty("RiseFallRate")?.SetValue(item, 0.02m);
        itemType.GetProperty("LastPrice")?.SetValue(item, 50000m);
        itemType.GetProperty("Volume24")?.SetValue(item, 1000m);

        list.Add(item);
        dataProperty.SetValue(response, list);
        return response;
    }

    public virtual Task<T?> GetVnDirectStockPricesAllAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectRatiosLatestByItemCodeAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectRecommendationsAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectTechnicalSignalsAsync<T>(string strategy, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetVnDirectChangePricesAsync<T>(CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetTwentyFourHMoneyTransactionListSsiAsync<T>(string symbol, int page = 1, int perPage = 1000, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetChainBrokerUnlocksListAsync<T>(int? page = null, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetChainBrokerProjectsListAsync<T>(int? page = null, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<T?> GetChainBrokerFundsListAsync<T>(int? page = null, CancellationToken cancellationToken = default) => Task.FromResult((T?)(object?)null);
    public virtual Task<IReadOnlyList<Power655ResultItem>?> GetPower655ResultsAsync(
        string dateFrom,
        string dateTo,
        CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<Power655ResultItem>?)null);
}
