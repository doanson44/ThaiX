using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Notifications;


namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private HttpResponseMessage HandleExternalData(string method, string path, string query, int pageNumber, int pageSize)
    {
        if (method == "POST")
        {
            if (path.Contains("yahoo-finance/vix/verdict", StringComparison.OrdinalIgnoreCase))
            {
                return DemoEnvelope.SuccessData(new VixVerdictDto
                {
                    Commentary = "Demo VIX verdict: market volatility is within a neutral regime. Risk-on bias remains acceptable with normal position sizing."
                });
            }

            // Sync triggers (chainbroker, tcbs-top10, etc.)
            return DemoEnvelope.SuccessData(new { synced = true, demo = true });
        }

        if (method != "GET")
        {
            return DemoEnvelope.SuccessData(new { synced = true, demo = true });
        }

        if (path.Contains("chainbroker/db/funds", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.Paged(_store.ChainBrokerFunds.ToList(), pageNumber, pageSize));
        }

        if (path.Contains("chainbroker/db/projects", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.Paged(_store.ChainBrokerProjects.ToList(), pageNumber, pageSize));
        }

        if (path.Contains("chainbroker/db/unlocks", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.Paged(_store.ChainBrokerUnlocks.ToList(), pageNumber, pageSize));
        }

        if (path.Contains("market/commodities", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new CommoditiesResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.Commodities.ToList()
            }));
        }

        if (path.Contains("market/currencies", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new CurrenciesResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.Currencies.ToList()
            }));
        }

        if (path.Contains("market/cryptocurrencies", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new CryptocurrenciesResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.Cryptocurrencies.ToList()
            }));
        }

        if (path.Contains("market/coingecko/coins-list", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new CoinGeckoCoinsListResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.CoinGeckoCoins.ToList()
            }));
        }

        if (path.Contains("market/coingecko/markets/", StringComparison.OrdinalIgnoreCase))
        {
            var coinId = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "bitcoin";
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoCoinMarket(coinId));
        }

        if (path.Contains("bank-interest-rates", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new BankInterestRatesResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.BankInterestRates.ToList()
            }));
        }

        if (path.Contains("bank-deposit-rates", StringComparison.OrdinalIgnoreCase))
        {
            var isCounterChannel = query.Contains(BankDepositRateTypes.Offline, StringComparison.OrdinalIgnoreCase);

            return _store.WithLock(() => DemoEnvelope.SuccessData(new BankDepositRatesResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Channel = isCounterChannel ? BankDepositRateTypes.Offline : BankDepositRateTypes.Online,
                Data = (isCounterChannel ? _store.BankDepositRatesOffline : _store.BankDepositRatesOnline).ToList()
            }));
        }

        if (path.Contains("sacombank/exchange-rates", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new SacombankExchangeRatesResponseDto
            {
                Success = true,
                Message = "Demo mode",
                UpdateDate = DateTime.UtcNow,
                ExchangeRates = _store.SacombankExchangeRates.ToList()
            }));
        }

        if (path.Contains("market/vndirect/change-prices", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() =>
            {
                var data = _store.VnDirectChangePrices.ToList();
                return DemoEnvelope.SuccessData(new VnDirectChangePricesResponseDto
                {
                    CurrentPage = 1,
                    Size = data.Count,
                    TotalElements = data.Count,
                    TotalPages = 1,
                    Data = data,
                    Success = true,
                    Message = "Demo mode"
                });
            });
        }

        if (path.Contains("market/vndirect/top-stocks", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() =>
            {
                var data = _store.VnDirectTopStocks.ToList();
                return DemoEnvelope.SuccessData(new VnDirectTopStocksResponseDto
                {
                    CurrentPage = 1,
                    Size = data.Count,
                    TotalElements = data.Count,
                    TotalPages = 1,
                    Data = data,
                    Success = true,
                    Message = "Demo mode"
                });
            });
        }

        if (path.Contains("mexc/contract-ticker", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new MexcContractTickersResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.MexcContractTickers.ToList()
            }));
        }

        if (path.Contains("mexc/spot-ticker-24hr", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new MexcSpotTicker24HrResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.MexcSpotTickers.ToList()
            }));
        }

        if (path.Contains("yahoo-finance/vix", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoVix());
        }

        if (path.Contains("tcbs-top10/portfolios", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() =>
            {
                var portfolios = _store.TcbsTop10Portfolios.ToList();
                var current = portfolios
                    .SelectMany(p => p.AddedTickers)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(40)
                    .ToList();
                var allTime = portfolios
                    .SelectMany(p => p.AddedTickers.Concat(p.RemovedTickers))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return DemoEnvelope.SuccessData(new TcbsTop10PortfoliosResult
                {
                    Portfolios = portfolios,
                    CurrentHoldings = current,
                    AllTimeHoldings = allTime
                });
            });
        }

        if (path.Contains("dragon-capital/fund-portfolio/", StringComparison.OrdinalIgnoreCase))
        {
            var fundCode = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "DCDS";
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoDragonCapital(fundCode));
        }

        if (path.Contains("market/24hmoney/transactions/", StringComparison.OrdinalIgnoreCase) &&
            path.Contains("/history", StringComparison.OrdinalIgnoreCase))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var symbolIndex = Array.FindIndex(segments, s => s.Equals("transactions", StringComparison.OrdinalIgnoreCase)) + 1;
            var symbol = symbolIndex > 0 && symbolIndex < segments.Length ? segments[symbolIndex] : "VNM";
            return DemoEnvelope.SuccessData(new TwentyFourHMoneyTransactionHistoryResponseDto
            {
                Symbol = symbol,
                CurrentPrice = 42.5m,
                Page = pageNumber,
                PerPage = pageSize <= 0 ? 50 : pageSize,
                TotalCount = DemoDataGenerators.DefaultListCount,
                Success = true,
                Message = "Demo mode",
                PriceComparison = BuildPriceComparisonPeriods()
            });
        }

        if (path.Contains("market/24hmoney/transactions/", StringComparison.OrdinalIgnoreCase))
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var symbolIndex = Array.FindIndex(segments, s => s.Equals("transactions", StringComparison.OrdinalIgnoreCase)) + 1;
            var symbol = symbolIndex > 0 && symbolIndex < segments.Length ? segments[symbolIndex] : "VNM";
            var perPage = pageSize <= 0 ? 50 : pageSize;
            var rows = Enumerable.Range(0, perPage).Select(i => new TwentyFourHMoneyTransactionDto
            {
                Time = DateTime.UtcNow.AddMinutes(-i),
                Price = 40m + (i % 10) * 0.1m,
                Volume = 1_000 + i * 10,
                Side = i % 2 == 0 ? "Buy" : "Sell"
            }).ToList();
            return DemoEnvelope.SuccessData(new TwentyFourHMoneyTransactionsResponseDto
            {
                Symbol = symbol,
                Page = pageNumber,
                PerPage = perPage,
                TotalCount = DemoDataGenerators.DefaultListCount,
                Data = rows,
                Success = true,
                Message = "Demo mode"
            });
        }

        if (path.Contains("binance/funding-rates", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("mexc/funding-rate", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoFundingRates());
        }

        if (path.Contains("binance/futures-ticker-24hr", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("binance/spot-ticker-24hr", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("bybit/linear-tickers", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("bybit/spot-tickers", StringComparison.OrdinalIgnoreCase))
        {
            var symbol = GetQueryValue(query, "symbol");
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoExchangeTickers(symbol));
        }

        if (path.Contains("binance/futures-depth/", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("binance/spot-depth/", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("mexc/contract-depth/", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("mexc/spot-depth/", StringComparison.OrdinalIgnoreCase))
        {
            var symbol = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "BTCUSDT";
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoOrderBook(symbol));
        }

        if (path.Contains("mexc/contract-kline/", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("mexc/spot-klines/", StringComparison.OrdinalIgnoreCase))
        {
            var symbol = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "BTC_USDT";
            var interval = GetQueryValue(query, "interval")
                           ?? (path.Contains("spot-klines", StringComparison.OrdinalIgnoreCase) ? "1m" : "Min1");
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoKlines(symbol, interval));
        }

        if (path.Contains("market/price-history/", StringComparison.OrdinalIgnoreCase))
        {
            var symbol = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "VNM";
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoStockPriceHistory(symbol));
        }

        if (path.Contains("market/watchlist-price/", StringComparison.OrdinalIgnoreCase))
        {
            var symbol = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "VNM";
            return DemoEnvelope.SuccessData(DemoDataGenerators.DemoWatchlistPrice(symbol));
        }

        if (path.Contains("market/vndirect/events", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new VnDirectEventsResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = new[] { "VNM", "VIC", "MWG", "FPT", "HPG" }
                    .Select((code, i) => new VnDirectEventDto
                    {
                        Code = code,
                        EventType = i % 2 == 0 ? "alert" : "noticed",
                        Title = $"{code} demo event",
                        Description = "Demo VnDirect market event",
                        EventDate = DateTime.UtcNow.AddDays(-i)
                    })
                    .ToList()
            });
        }

        if (path.Contains("market/vndirect/ratios/latest/", StringComparison.OrdinalIgnoreCase))
        {
            var code = path.EndsWith("/market", StringComparison.OrdinalIgnoreCase)
                ? "MARKET"
                : path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "VNM";
            return DemoEnvelope.SuccessData(new VnDirectRatiosResponseDto
            {
                Code = code,
                Success = true,
                Message = "Demo mode",
                Data =
                [
                    new VnDirectRatioItemDto { ItemCode = "PE", ItemName = "P/E", Value = 12.5m, Unit = "x" },
                    new VnDirectRatioItemDto { ItemCode = "PB", ItemName = "P/B", Value = 2.1m, Unit = "x" },
                    new VnDirectRatioItemDto { ItemCode = "ROE", ItemName = "ROE", Value = 18.4m, Unit = "%" }
                ]
            });
        }

        if (path.Contains("market/vndirect/recommendations", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new VnDirectRecommendationsResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = new[] { "VNM", "FPT", "MWG" }
                    .Select((code, i) => new VnDirectRecommendationDto
                    {
                        Code = code,
                        Recommendation = i % 2 == 0 ? "Buy" : "Hold",
                        TargetPrice = 50m + i * 5,
                        Analyst = "Demo Analyst",
                        PublishedAt = DateTime.UtcNow.AddDays(-i)
                    })
                    .ToList()
            });
        }

        if (path.Contains("market/vndirect/stock-prices/", StringComparison.OrdinalIgnoreCase))
        {
            var code = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "VNM";
            var history = DemoDataGenerators.DemoStockPriceHistory(code);
            return DemoEnvelope.SuccessData(new VnDirectStockPricesResponseDto
            {
                Code = code,
                Data = history.Data,
                Success = true,
                Message = "Demo mode"
            });
        }

        if (path.Contains("market/vndirect/technical-signals", StringComparison.OrdinalIgnoreCase))
        {
            var strategy = GetQueryValue(query, "strategy") ?? "cipLong";
            return DemoEnvelope.SuccessData(new VnDirectTechnicalSignalsResponseDto
            {
                Strategy = strategy,
                Success = true,
                Message = "Demo mode",
                Data = new[] { "VNM", "VIC", "HPG", "FPT" }
                    .Select((code, i) => new VnDirectTechnicalSignalDto
                    {
                        Code = code,
                        Signal = i % 2 == 0 ? "Long" : "Neutral",
                        Score = 60 + i,
                        SignalTime = DateTime.UtcNow.AddHours(-i)
                    })
                    .ToList()
            });
        }

        if (path.Contains("lottery/power-655", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() => DemoEnvelope.SuccessData(new Power655ResultsResponseDto
            {
                Success = true,
                Message = "Demo mode",
                Data = _store.Power655Draws.Take(30).Select(d => new Power655ResultDrawDto
                {
                    DrawDate = d.DrawDate,
                    Numbers = d.Numbers,
                    BonusNum = d.BonusNum,
                    Jackpot1Value = d.Jackpot1Value,
                    Jackpot2Value = d.Jackpot2Value
                }).ToList()
            }));
        }

        return DemoEnvelope.SuccessData(new
        {
            success = true,
            message = "Demo mode",
            data = Array.Empty<object>(),
            demo = true,
            path
        });
    }

    private static List<PriceComparisonPeriodDto> BuildPriceComparisonPeriods()
    {
        var periods = new[] { "7D", "30D", "90D", "180D", "1Y" };
        return periods.Select((period, i) => new PriceComparisonPeriodDto
        {
            Period = period,
            FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(i + 1) * 30)),
            ToDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TransactionCount = 1000 * (i + 1),
            TotalVolume = 5_000_000L * (i + 1),
            BetterVolume = 3_000_000L * (i + 1),
            WorseVolume = 1_500_000L * (i + 1),
            EqualVolume = 500_000L * (i + 1),
            BetterPercent = 60,
            WorsePercent = 30,
            AveragePrice = 40m + i,
            Top3Highest =
            [
                new PriceVolumePointDto { Price = 50 + i, Volume = 100_000 },
                new PriceVolumePointDto { Price = 48 + i, Volume = 90_000 },
                new PriceVolumePointDto { Price = 46 + i, Volume = 80_000 }
            ],
            Top3Lowest =
            [
                new PriceVolumePointDto { Price = 30 + i, Volume = 70_000 },
                new PriceVolumePointDto { Price = 32 + i, Volume = 60_000 },
                new PriceVolumePointDto { Price = 34 + i, Volume = 50_000 }
            ]
        }).ToList();
    }

}
