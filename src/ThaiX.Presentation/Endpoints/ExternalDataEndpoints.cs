using Hangfire;
using MediatR;
using System.Globalization;
using ThaiX.Application.Common.Enums;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.Banking.Queries.GetSacombankExchangeRates;
using ThaiX.Application.Features.ExternalData.Banking.Queries.GetVnExpressBankRates;
using ThaiX.Application.Features.ExternalData.BankInterestRates.Queries.GetBankInterestRates;
using ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerFundsFromDb;
using ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerProjectsFromDb;
using ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerUnlocksFromDb;
using ThaiX.Application.Features.ExternalData.Funds.Queries.GetDragonCapitalFundPortfolio;
using ThaiX.Application.Features.ExternalData.Lottery.Queries.GetPower655Results;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFundingRates;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesDepth;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24Hr;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24HrBySymbol;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotDepth;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24Hr;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24HrBySymbol;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBybitLinearTickers;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBybitSpotTickers;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCoinGeckoCoinMarket;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCoinGeckoCoinsList;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCommodities;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCryptocurrencies;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetCurrencies;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractDepth;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractFundingRates;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractKline;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickerBySymbol;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickers;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotDepth;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotKlines;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetStockPriceHistory;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetStockWatchlistPrice;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetTwentyFourHMoneyTransactionHistory;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetTwentyFourHMoneyTransactions;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectChangePrices;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatest;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRatiosLatestByItemCode;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectRecommendations;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectStockPrices;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTopStocks;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetYahooFinanceVix;
using ThaiX.Application.Features.TcbsTop10.Queries.GetTcbsTop10Portfolios;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// External data endpoints for third-party API integrations.
/// Includes CafeF, Binance, Bybit, MEXC, VnDirect, CoinGecko, Dragon Capital, Sacombank, Yahoo Finance, VnExpress, and iWealth Club APIs.
/// </summary>
public static class ExternalDataEndpoints
{
    public static void MapExternalDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        MapBankInterestRatesEndpoints(endpoints);
        MapBankDepositRatesEndpoints(endpoints);
        MapMarketDataEndpoints(endpoints);
        MapBybitEndpoints(endpoints);
        MapBinanceEndpoints(endpoints);
        MapMexcEndpoints(endpoints);
        MapDragonCapitalEndpoints(endpoints);
        MapSacombankEndpoints(endpoints);
        MapYahooFinanceEndpoints(endpoints);
        MapChainBrokerEndpoints(endpoints);
        MapKetQuaDienToanEndpoints(endpoints);
        MapTcbsTop10Endpoints(endpoints);
    }

    private static void MapBankInterestRatesEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/bank-interest-rates")
            .WithTags("External Data - Bank Interest Rates");

        // GET /api/external-data/bank-interest-rates
        group.MapGet("/", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBankInterestRatesQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<BankInterestRatesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetBankInterestRates")
        .WithDescription("Get current bank interest rates from CafeF. Data is cached for 5 minutes.");
    }

    private static void MapBankDepositRatesEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/bank-deposit-rates")
            .WithTags("External Data - Bank Deposit Rates");

        // GET /api/external-data/bank-deposit-rates?type=online|offline
        group.MapGet("/", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken,
            string? type = null) =>
        {
            if (!TryParseChannel(type, out var channel))
            {
                return Results.BadRequest(ApiResponse.ErrorResult(
                    "INVALID_BANK_RATE_TYPE",
                    $"Unsupported bank rate type '{type}'. Use 'online' or 'offline'."));
            }

            var result = await mediator.Send(new GetVnExpressBankRatesQuery { Channel = channel }, cancellationToken);

            var response = ApiResponse<VnExpressBankRatesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetBankDepositRates")
        .WithDescription("Get bank deposit interest rates from VnExpress for one channel (type=online|offline). Data is cached for 5 minutes.");
    }

    /// <summary>Parses the channel query parameter; null defaults to the online channel.</summary>
    private static bool TryParseChannel(string? type, out BankRateChannel channel)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            channel = BankRateChannel.Online;
            return true;
        }

        return Enum.TryParse(type, ignoreCase: true, out channel) && Enum.IsDefined(channel);
    }

    private static void MapMarketDataEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/market")
            .WithTags("External Data - Market Data");

        // GET /api/external-data/market/commodities
        group.MapGet("/commodities", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCommoditiesQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<CommoditiesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetCommodities")
        .WithDescription("Get current commodity prices (gold, silver, oil, metals, agricultural products) from CafeF. Data is cached for 5 minutes.");

        // GET /api/external-data/market/currencies
        group.MapGet("/currencies", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCurrenciesQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<CurrenciesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetCurrencies")
        .WithDescription("Get current currency exchange rates (USD, EUR, GBP, etc.) from CafeF. Data is cached for 5 minutes.");

        // GET /api/external-data/market/cryptocurrencies
        group.MapGet("/cryptocurrencies", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCryptocurrenciesQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<CryptocurrenciesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetCryptocurrencies")
        .WithDescription("Get current cryptocurrency prices (Bitcoin, Ethereum, etc.) from CafeF. Data is cached for 10 minutes.");

        // GET /api/external-data/market/coingecko/coins-list
        group.MapGet("/coingecko/coins-list", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCoinGeckoCoinsListQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<CoinGeckoCoinsListResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetCoinGeckoCoinsList")
        .WithDescription("Get CoinGecko coins list (id, symbol, name). Data is cached for 1 hour.");

        // GET /api/external-data/market/coingecko/markets/{coinId}
        group.MapGet("/coingecko/markets/{coinId}", async (
            string coinId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCoinGeckoCoinMarketQuery { CoinId = coinId };
            var result = await mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<CoinGeckoCoinMarketResponse>.ErrorResult(
                    "INVALID_COIN_ID",
                    result.Message ?? "Invalid coin id");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<CoinGeckoCoinMarketResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetCoinGeckoCoinMarketById")
        .WithDescription("Get CoinGecko coin market data by coin id from /coins/markets (example: bitcoin). coinId should come from /coingecko/coins-list.");

        // GET /api/external-data/market/price-history/{symbol}
        group.MapGet("/price-history/{symbol}", async (
            string symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStockPriceHistoryQuery { Symbol = symbol };
            var result = await mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<StockPriceHistoryResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    result.Message ?? "Invalid stock symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<StockPriceHistoryResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetStockPriceHistory")
        .WithDescription("Get stock price history by symbol from CafeF (examples: VNM, VIC, NKG). Data is cached for 5 minutes.");

        // GET /api/external-data/market/watchlist-price/{symbol}
        group.MapGet("/watchlist-price/{symbol}", async (
            string symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStockWatchlistPriceQuery { Symbol = symbol };
            var result = await mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<StockWatchlistPriceResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    result.Message ?? "Invalid stock symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<StockWatchlistPriceResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetStockWatchlistPrice")
        .WithDescription("Get current watchlist price snapshot by symbol from CafeF (examples: VNM, VIC, NKG). Data is cached for 30 seconds.");

        // GET /api/external-data/market/24hmoney/transactions/{symbol}?page=1&perPage=1000
        group.MapGet("/24hmoney/transactions/{symbol}", async (
            string symbol,
            int? page,
            int? perPage,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetTwentyFourHMoneyTransactionsQuery
            {
                Symbol = symbol,
                Page = page ?? 1,
                PerPage = perPage ?? 1000
            }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<TwentyFourHMoneyTransactionsResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    result.Message ?? "Invalid stock symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<TwentyFourHMoneyTransactionsResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetTwentyFourHMoneyTransactions")
        .WithDescription("Get 24HMoney SSI transaction list by stock symbol (example: MWG). Supports page and perPage query parameters.");

        // GET /api/external-data/market/24hmoney/transactions/{symbol}/history?currentPrice=71.5&page=1&perPage=50
        group.MapGet("/24hmoney/transactions/{symbol}/history", async (
            string symbol,
            string? currentPrice,
            int? page,
            int? perPage,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!TryParseFlexibleDecimal(currentPrice, out var parsedCurrentPrice))
            {
                var errorResponse = ApiResponse<TwentyFourHMoneyTransactionHistoryResponse>.ErrorResult(
                    "INVALID_REQUEST",
                    "Invalid currentPrice format. Supported examples: 68.0 or 68,0");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var result = await mediator.Send(new GetTwentyFourHMoneyTransactionHistoryQuery
            {
                Symbol = symbol,
                CurrentPrice = parsedCurrentPrice,
                Page = page ?? 1,
                PerPage = perPage ?? 50
            }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<TwentyFourHMoneyTransactionHistoryResponse>.ErrorResult(
                    "INVALID_REQUEST",
                    result.Message ?? "Invalid request");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<TwentyFourHMoneyTransactionHistoryResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetTwentyFourHMoneyTransactionHistory")
        .WithDescription("Get locally-stored 24HMoney transaction history for a symbol (synced daily by " +
            "TwentyFourHMoneyTransactionSyncJob, up to 1 year retained). Optionally pass currentPrice to compare it " +
            "against historical matched prices over 1D/1W/1M/3M/6M/1Y look-back windows: volume-weighted % of past " +
            "buyers who paid more (current price is better) or less (current price is worse) than currentPrice.");

        // GET /api/external-data/market/vndirect/top-stocks
        group.MapGet("/vndirect/top-stocks", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectTopStocksQuery(), cancellationToken);

            var response = ApiResponse<VnDirectTopStocksResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVnDirectTopStocks")
        .WithDescription("Get VnDirect top stocks list via proxy. Includes enrichment from technical signals, TCBS/Dragon Capital portfolio holdings, and VnDirect events. Data is cached for 15 minutes.");

        // GET /api/external-data/market/vndirect/ratios/latest/{code}
        group.MapGet("/vndirect/ratios/latest/{code}", async (
            string code,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectRatiosLatestQuery { Code = code }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<VnDirectRatiosLatestResponse>.ErrorResult(
                    "INVALID_CODE",
                    result.Message ?? "Invalid code");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<VnDirectRatiosLatestResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVnDirectRatiosLatest")
        .WithDescription("Get VnDirect latest ratios by stock code via proxy (example: VNM). Data is cached for 30 seconds.");

        // GET /api/external-data/market/vndirect/ratios/latest/market
        group.MapGet("/vndirect/ratios/latest/market", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectRatiosLatestByItemCodeQuery(), cancellationToken);

            var response = ApiResponse<VnDirectRatiosLatestByItemCodeResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVnDirectRatiosLatestByItemCode")
        .WithDescription("Get VnDirect latest market ratios by predefined item codes via proxy. Data is cached for 30 seconds.");

        // GET /api/external-data/market/vndirect/events
        group.MapGet("/vndirect/events", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectEventsQuery(), cancellationToken);

            var response = ApiResponse<VnDirectEventsResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVnDirectEvents")
        .WithDescription("Get VnDirect stock events (nomargin, alert, halt, control, suspend, noticed) via proxy. Data is cached for 30 seconds.");

        // GET /api/external-data/market/vndirect/stock-prices/{code}
        group.MapGet("/vndirect/stock-prices/{code}", async (
            string code,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectStockPricesQuery { Code = code }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<VnDirectStockPricesResponse>.ErrorResult(
                    "INVALID_CODE",
                    result.Message ?? "Invalid code");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<VnDirectStockPricesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVnDirectStockPrices")
        .WithDescription("Get VnDirect stock price history by stock code via proxy (example: VNM). Data is cached for 5 minutes.");

        // GET /api/external-data/market/vndirect/recommendations
        group.MapGet("/vndirect/recommendations", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectRecommendationsQuery(), cancellationToken);

            var response = ApiResponse<VnDirectRecommendationsResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVnDirectRecommendations")
        .WithDescription("Get VnDirect recommendations via proxy. Data is cached for 30 seconds.");

        // GET /api/external-data/market/vndirect/technical-signals
        group.MapGet("/vndirect/technical-signals", async (
            string? strategy,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectTechnicalSignalsQuery { Strategy = strategy }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<VnDirectTechnicalSignalsResponse>.ErrorResult(
                    "INVALID_STRATEGY",
                    result.Message ?? "Invalid strategy");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<VnDirectTechnicalSignalsResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVnDirectTechnicalSignals")
        .WithDescription("Get VnDirect technical signals via proxy. Optional query parameter: strategy (default: cipLong, example: cipShort). Data is cached for 30 seconds.");

        // GET /api/external-data/market/vndirect/change-prices
        group.MapGet("/vndirect/change-prices", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetVnDirectChangePricesQuery(), cancellationToken);
            var response = ApiResponse<VnDirectChangePricesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetVnDirectChangePrices")
        .WithDescription("Get VnDirect change prices for VNINDEX, HNX, UPCOM, VN30, VN30F1M (period 1D) via proxy. Data is cached for 30 seconds.");
    }

    private static void MapBybitEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/bybit")
            .WithTags("External Data - Bybit");

        // GET /api/external-data/bybit/linear-tickers
        group.MapGet("/linear-tickers", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBybitLinearTickersQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<BybitLinearTickersResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBybitLinearTickers")
        .WithDescription("Get Bybit linear/perpetual market tickers. Data is cached for 1 minute.");

        // GET /api/external-data/bybit/spot-tickers
        group.MapGet("/spot-tickers", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBybitSpotTickersQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<BybitSpotTickersResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBybitSpotTickers")
        .WithDescription("Get Bybit spot market tickers. Data is cached for 1 minute.");
    }

    private static void MapBinanceEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/binance")
            .WithTags("External Data - Binance");

        // GET /api/external-data/binance/funding-rates
        group.MapGet("/funding-rates", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetBinanceFundingRatesQuery(), cancellationToken);

            var response = ApiResponse<BinanceFundingRatesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBinanceFundingRates")
        .WithDescription("Get Binance funding rate history (limit=1000). Data is cached for 1 minute.");

        // GET /api/external-data/binance/futures-ticker-24hr
        // GET /api/external-data/binance/futures-ticker-24hr?symbol=BTCUSDT
        group.MapGet("/futures-ticker-24hr", async (
            string? symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                var listResult = await mediator.Send(new GetBinanceFuturesTicker24HrQuery(), cancellationToken);

                var listResponse = ApiResponse<BinanceFuturesTicker24HrResponse>.SuccessResult(listResult);
                listResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();

                return Results.Ok(listResponse);
            }

            var bySymbolResult = await mediator.Send(
                new GetBinanceFuturesTicker24HrBySymbolQuery { Symbol = symbol },
                cancellationToken);

            if (!bySymbolResult.Success)
            {
                var errorResponse = ApiResponse<BinanceFuturesTicker24HrBySymbolResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    bySymbolResult.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var bySymbolResponse = ApiResponse<BinanceFuturesTicker24HrBySymbolResponse>.SuccessResult(bySymbolResult);
            bySymbolResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(bySymbolResponse);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBinanceFuturesTicker24Hr")
        .WithDescription("Get Binance futures 24hr tickers. Without symbol returns all symbols; with symbol query returns one ticker (example: BTCUSDT).");

        // GET /api/external-data/binance/spot-depth/{symbol}
        group.MapGet("/spot-depth/{symbol}", async (
            string symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetBinanceSpotDepthQuery { Symbol = symbol }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<BinanceSpotDepthResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    result.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<BinanceSpotDepthResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBinanceSpotDepth")
        .WithDescription("Get Binance spot order book depth by symbol with fixed limit=5000 (example: BTCUSDT).");

        // GET /api/external-data/binance/futures-depth/{symbol}
        group.MapGet("/futures-depth/{symbol}", async (
            string symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetBinanceFuturesDepthQuery { Symbol = symbol }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<BinanceFuturesDepthResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    result.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<BinanceFuturesDepthResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBinanceFuturesDepth")
        .WithDescription("Get Binance futures order book depth by symbol with fixed limit=1000 (example: BTCUSDT).");

        // GET /api/external-data/binance/spot-ticker-24hr
        // GET /api/external-data/binance/spot-ticker-24hr?symbol=BTCUSDT
        group.MapGet("/spot-ticker-24hr", async (
            string? symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                var listResult = await mediator.Send(new GetBinanceSpotTicker24HrQuery(), cancellationToken);

                var listResponse = ApiResponse<BinanceSpotTicker24HrResponse>.SuccessResult(listResult);
                listResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();

                return Results.Ok(listResponse);
            }

            var bySymbolResult = await mediator.Send(
                new GetBinanceSpotTicker24HrBySymbolQuery { Symbol = symbol },
                cancellationToken);

            if (!bySymbolResult.Success)
            {
                var errorResponse = ApiResponse<BinanceSpotTicker24HrBySymbolResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    bySymbolResult.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var bySymbolResponse = ApiResponse<BinanceSpotTicker24HrBySymbolResponse>.SuccessResult(bySymbolResult);
            bySymbolResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(bySymbolResponse);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBinanceSpotTicker24Hr")
        .WithDescription("Get Binance spot 24hr tickers. Without symbol returns all symbols; with symbol query returns one ticker (example: BTCUSDT).");
    }

    private static void MapMexcEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/mexc")
            .WithTags("External Data - MEXC");

        // GET /api/external-data/mexc/contract-ticker
        // GET /api/external-data/mexc/contract-ticker?symbol=BTC_USDT
        group.MapGet("/contract-ticker", async (
            string? symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                var allTickersResult = await mediator.Send(new GetMexcContractTickersQuery(), cancellationToken);

                var allTickersResponse = ApiResponse<MexcContractTickersResponse>.SuccessResult(allTickersResult);
                allTickersResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();

                return Results.Ok(allTickersResponse);
            }

            var bySymbolQuery = new GetMexcContractTickerBySymbolQuery { Symbol = symbol };
            var bySymbolResult = await mediator.Send(bySymbolQuery, cancellationToken);

            if (!bySymbolResult.Success)
            {
                var errorResponse = ApiResponse<MexcContractTickerBySymbolResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    bySymbolResult.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var bySymbolResponse = ApiResponse<MexcContractTickerBySymbolResponse>.SuccessResult(bySymbolResult);
            bySymbolResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(bySymbolResponse);
        })
        .AllowAnonymous()
        .WithName("GetMexcContractTicker")
        .WithDescription("Get MEXC contract ticker data. Without symbol returns all market tickers; with symbol query (example: BTC_USDT) returns one ticker.");

        // GET /api/external-data/mexc/contract-kline/{symbol}
        // GET /api/external-data/mexc/contract-kline/{symbol}?interval=Min15
        group.MapGet("/contract-kline/{symbol}", async (
            string symbol,
            string? interval,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMexcContractKlineQuery
            {
                Symbol = symbol,
                Interval = interval
            };

            var result = await mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<MexcContractKlineResponse>.ErrorResult(
                    "INVALID_KLINE_REQUEST",
                    result.Message ?? "Invalid kline request");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<MexcContractKlineResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetMexcContractKline")
        .WithDescription("Get MEXC contract kline data by symbol with optional interval. Supported interval: Min1, Min5, Min15, Min30, Min60, Hour4, Hour8, Day1, Week1, Month1. Defaults to Min1 when omitted.");

        // GET /api/external-data/mexc/funding-rate
        group.MapGet("/funding-rate", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetMexcContractFundingRatesQuery(), cancellationToken);

            var response = ApiResponse<MexcContractFundingRatesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetMexcContractFundingRates")
        .WithDescription("Get MEXC contract funding rates for all symbols.");

        // GET /api/external-data/mexc/contract-depth/{symbol}
        group.MapGet("/contract-depth/{symbol}", async (
            string symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMexcContractDepthQuery { Symbol = symbol };
            var result = await mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<MexcContractDepthResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    result.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<MexcContractDepthResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetMexcContractDepth")
        .WithDescription("Get MEXC contract order book depth by symbol (example: BTC_USDT).");

        // GET /api/external-data/mexc/spot-ticker-24hr
        // GET /api/external-data/mexc/spot-ticker-24hr?symbol=BTCUSDT
        group.MapGet("/spot-ticker-24hr", async (
            string? symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                var allResult = await mediator.Send(new GetMexcSpotTicker24HrQuery(), cancellationToken);

                var allResponse = ApiResponse<MexcSpotTicker24HrResponse>.SuccessResult(allResult);
                allResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();

                return Results.Ok(allResponse);
            }

            var bySymbolResult = await mediator.Send(
                new GetMexcSpotTicker24HrBySymbolQuery { Symbol = symbol },
                cancellationToken);

            if (!bySymbolResult.Success)
            {
                var errorResponse = ApiResponse<MexcSpotTicker24HrBySymbolResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    bySymbolResult.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<MexcSpotTicker24HrBySymbolResponse>.SuccessResult(bySymbolResult);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetMexcSpotTicker24Hr")
        .WithDescription("Get MEXC spot 24h ticker statistics. Without symbol returns all symbols; with symbol query (example: BTCUSDT) returns one ticker.");

        // GET /api/external-data/mexc/spot-klines/{symbol}?interval=1m
        group.MapGet("/spot-klines/{symbol}", async (
            string symbol,
            string? interval,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetMexcSpotKlinesQuery
            {
                Symbol = symbol,
                Interval = interval
            }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<MexcSpotKlinesResponse>.ErrorResult(
                    "INVALID_KLINE_REQUEST",
                    result.Message ?? "Invalid kline request");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<MexcSpotKlinesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetMexcSpotKlines")
        .WithDescription("Get MEXC spot klines by symbol and interval. Supported interval: 1m, 5m, 15m, 30m, 60m, 4h, 1d, 1W, 1M.");

        // GET /api/external-data/mexc/spot-depth/{symbol}
        group.MapGet("/spot-depth/{symbol}", async (
            string symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetMexcSpotDepthQuery { Symbol = symbol }, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<MexcSpotDepthResponse>.ErrorResult(
                    "INVALID_SYMBOL",
                    result.Message ?? "Invalid symbol");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<MexcSpotDepthResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetMexcSpotDepth")
        .WithDescription("Get MEXC spot depth by symbol with fixed limit=5000.");
    }

    private static void MapDragonCapitalEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/dragon-capital")
            .WithTags("External Data - Dragon Capital");

        // GET /api/external-data/dragon-capital/fund-portfolio/{fundCode}
        group.MapGet("/fund-portfolio/{fundCode}", async (
            string fundCode,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDragonCapitalFundPortfolioQuery { FundCode = fundCode };
            var result = await mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                var errorResponse = ApiResponse<DragonCapitalFundPortfolioResponse>.ErrorResult(
                    "INVALID_FUND_CODE",
                    result.Message ?? "Invalid fund code");
                errorResponse.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.BadRequest(errorResponse);
            }

            var response = ApiResponse<DragonCapitalFundPortfolioResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetDragonCapitalFundPortfolio")
        .WithDescription("Get Dragon Capital fund portfolio data. Supported fund codes: VF1, VF4, VFMVN30, VFMVND, VFMMID. Data is cached for 1 hour.");
    }

    private static void MapSacombankEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/sacombank")
            .WithTags("External Data - Sacombank");

        // GET /api/external-data/sacombank/exchange-rates
        group.MapGet("/exchange-rates", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSacombankExchangeRatesQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<SacombankExchangeRatesResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetSacombankExchangeRates")
        .WithDescription("Get Sacombank exchange rates for currencies and gold (SJC, SBJ). Data is cached for 5 minutes.");
    }

    private static void MapYahooFinanceEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/yahoo-finance")
            .WithTags("External Data - Yahoo Finance");

        // GET /api/external-data/yahoo-finance/vix
        group.MapGet("/vix", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetYahooFinanceVixQuery();
            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<YahooFinanceVixResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetYahooFinanceVix")
        .WithDescription("Get CBOE Volatility Index (VIX) data from Yahoo Finance. Data is cached for 5 minutes.");

        // POST /api/external-data/yahoo-finance/vix/verdict — AI market commentary on an
        // already-fetched VIX snapshot. Thin passthrough to IAiTextGenerationService, mirrors
        // ResumeAiEndpoints.cs/TradingEndpoints.cs verdict route — no MediatR/CQRS needed since
        // this doesn't touch the database.
        group.MapPost("/vix/verdict", async (
            GetVixVerdictRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(25));

            var systemPrompt = (
                "You are a macro markets analyst commenting on the CBOE Volatility Index (VIX) for a " +
                "retail trader/investor. Based on the data given, explain what the current VIX level and " +
                "regime mean for risk appetite, liquidity, and capital flows right now, and give a direct, " +
                "practical takeaway for someone managing a stock/crypto portfolio. Be concrete and confident — " +
                "no wishy-washy disclaimers. Return 2-4 sentences, no preamble.")
                .WithCurrentCultureInstruction();

            var prompt =
                $"Symbol: {request.Symbol}\n" +
                $"Current Value: {Fmt(request.CurrentValue)}\n" +
                $"EMA10: {Fmt(request.Ema10)}\n" +
                $"EMA20: {Fmt(request.Ema20)}\n" +
                $"Momentum 3D: {Fmt(request.Momentum3D)}\n" +
                $"Momentum 5D: {Fmt(request.Momentum5D)}\n" +
                $"Percentile (vs history): {request.Percentile:P0}\n" +
                $"Is Spike: {request.IsSpike}\n" +
                $"Regime: {request.Regime}";

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);

            var response = ApiResponse<VixVerdictResponse>.SuccessResult(new VixVerdictResponse
            {
                Commentary = result.Text.Trim()
            });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetVixVerdict")
        .WithDescription("Decisive AI commentary on VIX regime/risk implications, based on an already-fetched VIX snapshot.");
    }

    private static string Fmt(decimal? value) => value?.ToString() ?? "n/a";

    private static bool TryParseFlexibleDecimal(string? raw, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        var text = raw.Trim();

        // Try InvariantCulture first — handles 37.0, 68.5 (dot = decimal, the common URL format)
        if (decimal.TryParse(
                text,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var parsed))
        {
            value = parsed;
            return true;
        }

        // Fallback: vi-VN for comma-as-decimal inputs like 37,0 or 68,5
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("vi-VN"), out parsed))
        {
            value = parsed;
            return true;
        }

        // Last attempt: replace comma with dot and retry InvariantCulture
        if (text.Contains(',') && !text.Contains('.'))
        {
            text = text.Replace(',', '.');
            if (decimal.TryParse(
                    text,
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out parsed))
            {
                value = parsed;
                return true;
            }
        }

        return false;
    }

    private static void MapChainBrokerEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/chainbroker")
            .WithTags("External Data - ChainBroker");

        // POST /api/external-data/chainbroker/sync/funds
        group.MapPost("/sync/funds", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.ChainBrokerFundsSyncJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted(null, response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ChainBrokerDataSync)
        .WithName("TriggerChainBrokerFundsSync")
        .WithDescription("Manually trigger the ChainBroker funds sync job. Returns the Hangfire job ID. Requires ChainBrokerData.Sync permission.");

        // POST /api/external-data/chainbroker/sync/projects
        group.MapPost("/sync/projects", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.ChainBrokerProjectsSyncJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted(null, response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ChainBrokerDataSync)
        .WithName("TriggerChainBrokerProjectsSync")
        .WithDescription("Manually trigger the ChainBroker projects sync job. Returns the Hangfire job ID. Requires ChainBrokerData.Sync permission.");

        // POST /api/external-data/chainbroker/sync/unlocks
        group.MapPost("/sync/unlocks", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.ChainBrokerUnlocksSyncJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted(null, response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ChainBrokerDataSync)
        .WithName("TriggerChainBrokerUnlocksSync")
        .WithDescription("Manually trigger the ChainBroker unlocks sync job. Returns the Hangfire job ID. Requires ChainBrokerData.Sync permission.");

        // POST /api/external-data/chainbroker/sync (deprecated - kept for backward compatibility)
        group.MapPost("/sync", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            // Enqueue all 3 sync jobs in sequence: funds -> projects -> unlocks
            var fundJobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.ChainBrokerFundsSyncJob>(
                job => job.RunAsync(CancellationToken.None));
            var projectJobId = jobClient.ContinueJobWith<Infrastructure.BackgroundJobs.ChainBrokerProjectsSyncJob>(
                fundJobId,
                job => job.RunAsync(CancellationToken.None));
            var unlockJobId = jobClient.ContinueJobWith<Infrastructure.BackgroundJobs.ChainBrokerUnlocksSyncJob>(
                projectJobId,
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { fundJobId, projectJobId, unlockJobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted(null, response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ChainBrokerDataSync)
        .WithName("TriggerChainBrokerSync")
        .WithDescription("DEPRECATED - use /sync/funds, /sync/projects, /sync/unlocks instead. Manually trigger the ChainBroker sync jobs in sequence. Returns Hangfire job IDs.");

        // GET /api/external-data/chainbroker/db/funds
        group.MapGet("/db/funds", async (
            string? search,
            int? page,
            int? pageSize,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetChainBrokerFundsFromDbQuery
            {
                Search = search,
                PageNumber = page ?? 1,
                PageSize = pageSize == -1 ? -1 : Math.Clamp(pageSize ?? 50, 1, 200)
            }, cancellationToken);

            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ChainBrokerDataRead)
        .WithName("GetChainBrokerFundsFromDb")
        .WithDescription("Get all ChainBroker funds from the local database (synced weekly). Supports optional search and pagination (page, pageSize, max 200). Use pageSize=-1 to return all records.");

        // GET /api/external-data/chainbroker/db/projects
        group.MapGet("/db/projects", async (
            string? search,
            int? page,
            int? pageSize,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetChainBrokerProjectsFromDbQuery
            {
                Search = search,
                PageNumber = page ?? 1,
                PageSize = pageSize == -1 ? -1 : Math.Clamp(pageSize ?? 50, 1, 200)
            }, cancellationToken);

            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ChainBrokerDataRead)
        .WithName("GetChainBrokerProjectsFromDb")
        .WithDescription("Get all ChainBroker projects from the local database (synced weekly), sorted by rank. Supports optional search (name/slug/ticker) and pagination. Use pageSize=-1 to return all records.");

        // GET /api/external-data/chainbroker/db/unlocks
        group.MapGet("/db/unlocks", async (
            string? search,
            int? page,
            int? pageSize,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetChainBrokerUnlocksFromDbQuery
            {
                Search = search,
                PageNumber = page ?? 1,
                PageSize = pageSize == -1 ? -1 : Math.Clamp(pageSize ?? 50, 1, 200)
            }, cancellationToken);

            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ChainBrokerDataRead)
        .WithName("GetChainBrokerUnlocksFromDb")
        .WithDescription("Get all ChainBroker token unlocks from the local database (synced weekly), sorted by nearest unlock date. Supports optional search (name/slug/ticker) and pagination.");
    }

    private static void MapKetQuaDienToanEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/lottery")
            .WithTags("External Data - Lottery");

        // GET /api/external-data/lottery/power-655?dateFrom=01-08-2017&dateTo=15-07-2026
        group.MapGet("/power-655", async (
            string? dateFrom,
            string? dateTo,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPower655ResultsQuery
            {
                DateFrom = dateFrom ?? "01-08-2017",
                DateTo = dateTo ?? DateTime.Now.ToString("dd-MM-yyyy")
            };

            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<Power655ResultsResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetPower655Results")
        .WithDescription("Get Power 6/55 lottery results from ketquadientoan.com. Date format: dd-MM-yyyy. Data is cached for 1 hour.");
    }

    private static void MapTcbsTop10Endpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/external-data/tcbs-top10")
            .WithTags("External Data - TCBS Top 10");

        // GET /api/external-data/tcbs-top10/portfolios
        group.MapGet("/portfolios", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetTcbsTop10PortfoliosQuery(), cancellationToken);

            var response = ApiResponse<TcbsTop10PortfoliosResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetTcbsTop10Portfolios")
        .WithDescription("Get all TCBS Top 10 monthly portfolio updates from the local database (synced daily), sorted by most recent first. Returns full data including added/removed tickers and image GUIDs.");

        // POST /api/external-data/tcbs-top10/sync
        group.MapPost("/sync", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.IWealthClubTop10SyncJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted(null, response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.TcbsTop10DataSync)
        .WithName("TriggerTcbsTop10Sync")
        .WithDescription("Manually trigger the TCBS Top 10 sync job. Returns the Hangfire job ID. Requires TcbsTop10Data.Sync permission.");
    }
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

public sealed record VixVerdictResponse
{
    public string Commentary { get; init; } = string.Empty;
}

