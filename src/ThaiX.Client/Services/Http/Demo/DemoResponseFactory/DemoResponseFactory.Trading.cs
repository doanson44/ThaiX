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
    private HttpResponseMessage HandleTrading(string method, string path, int pageNumber, int pageSize)
    {
        if (path.Contains("suggestion-history/export", StringComparison.OrdinalIgnoreCase))
        {
            var csv = "symbol,rank,entryPrice,signal,confidence\nBTC,1,65000,Long,0.82\nETH,2,3200,Long,0.75\n";
            var bytes = Encoding.UTF8.GetBytes(csv);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "weekly_suggestion_demo.csv"
            };
            return response;
        }

        if (path.Contains("suggestion-history/evaluate", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return _store.WithLock(() =>
            {
                var items = _store.WeeklySuggestionReports
                    .Take(20)
                    .SelectMany(r => r.Picks.Select(p => new EvaluatedWeeklySuggestionDto
                    {
                        Symbol = p.Symbol,
                        RunAtUtc = r.RunAtUtc,
                        Timeframe = p.Timeframe,
                        Rank = p.Rank,
                        EntryPrice = p.EntryPrice,
                        CurrentPrice = p.EntryPrice * 1.05m,
                        ChangePercentFromEntry = 5m,
                        ReportKey = r.ReportKey,
                        AssetClass = r.AssetClass
                    }))
                    .ToList();
                return DemoEnvelope.SuccessData(new EvaluateWeeklySuggestionPerformanceResultDto
                {
                    MatchedCount = items.Count,
                    Items = items
                });
            });
        }

        if (path.Contains("suggestion-history", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            return _store.WithLock(() =>
            {
                var all = _store.WeeklySuggestionReports.ToList();
                var size = pageSize <= 0 ? all.Count : pageSize;
                var page = Math.Max(1, pageNumber);
                var slice = all.Skip((page - 1) * size).Take(size).ToList();
                return DemoEnvelope.SuccessData(new WeeklySuggestionHistoryResultDto
                {
                    Page = page,
                    PageSize = size,
                    Total = all.Count,
                    Items = slice
                });
            });
        }

        if (path.Contains("weekly-suggestion/run", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return DemoEnvelope.SuccessData(new { started = true, demo = true });
        }

        if (path.EndsWith("api/trading/suggestion", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("api/trading/suggestion?", StringComparison.OrdinalIgnoreCase) ||
            (path.Contains("/suggestion", StringComparison.OrdinalIgnoreCase) &&
             !path.Contains("suggestion-history", StringComparison.OrdinalIgnoreCase) &&
             method == "GET"))
        {
            return DemoEnvelope.SuccessData(new TradeSuggestionDto
            {
                Trend = "Up",
                Momentum = "Strong",
                Setup = "Breakout",
                Signal = "Long",
                EntryPrice = 100m,
                StopLoss = 95m,
                TakeProfit1 = 110m,
                TakeProfit2 = 120m,
                Confidence = 0.78m
            });
        }

        if (path.Contains("/verdict", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return DemoEnvelope.SuccessData(new TradeVerdictDto
            {
                Verdict = "Proceed with caution",
                Reasoning = "Demo verdict: setup quality is acceptable; keep risk under 1% of equity."
            });
        }

        if (method == "POST")
        {
            return DemoEnvelope.SuccessData(new { started = true, demo = true });
        }

        return DemoEnvelope.SuccessData(new { demo = true, path });
    }

}
