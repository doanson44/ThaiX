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
    private HttpResponseMessage HandleTypedListResource(
        string method, string path, string query, int pageNumber, int pageSize)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (method == "GET" &&
            segments.Length >= 4 &&
            segments[^1].Equals("transactions", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(segments[^2], out _))
        {
            var transactions = Enumerable.Range(1, 8).Select(i => new PositionTransactionDto
            {
                Id = Guid.NewGuid(),
                TransactionType = i % 2 == 0 ? "Sell" : "Buy",
                Quantity = 10 * i,
                Price = 100 + i,
                Fee = 1.5m,
                TransactedAt = DateTime.UtcNow.AddDays(-i),
                Note = "Demo transaction",
                ExternalRef = $"DEMO-{i:D4}"
            }).ToList();
            return DemoEnvelope.Paged(transactions, pageNumber, pageSize);
        }

        Guid entityId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[^1], out entityId);

        if (method == "GET" && !hasId)
        {
            return ResolveTypedList(path, query, pageNumber, pageSize);
        }

        if (method == "GET" && hasId)
        {
            return ResolveTypedDetail(path, entityId);
        }

        if (method == "POST")
        {
            return DemoEnvelope.SuccessData(Guid.NewGuid());
        }

        return DemoEnvelope.Success();
    }

    private HttpResponseMessage ResolveTypedList(string path, string query, int pageNumber, int pageSize)
    {
        return _store.WithLock(() =>
        {
            var portfolioIdText = GetQueryValue(query, "portfolioId");
            Guid? portfolioId = Guid.TryParse(portfolioIdText, out var pid) ? pid : null;
            var search = GetSearchTerm(query);

            if (path.StartsWith("api/stock-positions", StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<StockPositionListItemDto> rows = portfolioId is null
                    ? _store.StockPositions
                    : _store.StockPositions.Where(x => x.PortfolioId == portfolioId);
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(x => MatchesSearch(search, x.Symbol, x.Exchange, x.Note));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            }

            if (path.StartsWith("api/crypto-positions", StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<CryptoPositionListItemDto> rows = portfolioId is null
                    ? _store.CryptoPositions
                    : _store.CryptoPositions.Where(x => x.PortfolioId == portfolioId);
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(x => MatchesSearch(search, x.Symbol, x.Note));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            }

            if (path.StartsWith("api/saving-positions", StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<SavingPositionListItemDto> rows = portfolioId is null
                    ? _store.SavingPositions
                    : _store.SavingPositions.Where(x => x.PortfolioId == portfolioId);
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(x => MatchesSearch(search, x.BankName, x.AccountNumber, x.Status, x.Note));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            }

            if (path.StartsWith("api/price-alerts", StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<PriceAlertListItemDto> rows = _store.PriceAlerts;
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(x => MatchesSearch(search, x.Symbol, x.AssetType, x.Condition, x.Note));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            }

            if (path.StartsWith("api/market-scanner", StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<MarketScannerRuleListItemDto> rows = _store.MarketScannerRules;
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(x => MatchesSearch(search, x.Name, x.SignalType, x.Window));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            }

            return DemoEnvelope.EmptyPaged(pageNumber, pageSize);
        });
    }

    private HttpResponseMessage ResolveTypedDetail(string path, Guid entityId)
    {
        return _store.WithLock(() =>
        {
            if (path.StartsWith("api/price-alerts", StringComparison.OrdinalIgnoreCase))
            {
                var item = _store.PriceAlerts.FirstOrDefault(x => x.Id == entityId) ?? _store.PriceAlerts[0];
                return DemoEnvelope.SuccessData(item);
            }

            if (path.StartsWith("api/market-scanner", StringComparison.OrdinalIgnoreCase))
            {
                var item = _store.MarketScannerRules.FirstOrDefault(x => x.Id == entityId) ?? _store.MarketScannerRules[0];
                return DemoEnvelope.SuccessData(item);
            }

            return DemoEnvelope.SuccessData(new { id = entityId, demo = true, path });
        });
    }

    private static bool IsListArea(string path)
    {
        string[] prefixes =
        [
            "api/stock-positions",
            "api/crypto-positions",
            "api/saving-positions",
            "api/price-alerts",
            "api/market-scanner"
        ];

        return prefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

}
