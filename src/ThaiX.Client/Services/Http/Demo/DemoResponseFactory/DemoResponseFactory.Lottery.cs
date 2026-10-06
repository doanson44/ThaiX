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
    private HttpResponseMessage HandleLottery(string method, string path)
    {
        if (method == "GET" && path.Contains("/analysis", StringComparison.OrdinalIgnoreCase))
        {
            return _store.WithLock(() =>
            {
                var draws = _store.Power655Draws.ToList();
                var freq = Enumerable.Range(1, 55)
                    .Select(n => new NumberFrequencyRecord
                    {
                        Number = n,
                        Frequency = draws.Count(d => d.Numbers.Contains(n))
                    })
                    .OrderByDescending(x => x.Frequency)
                    .ToList();

                return DemoEnvelope.SuccessData(new Power655AnalysisResponse
                {
                    Draws = draws,
                    TopMostFrequent = freq.Take(10).ToList(),
                    TopLeastFrequent = freq.OrderBy(x => x.Frequency).Take(10).ToList(),
                    Predictions = Enumerable.Range(1, 10).Select(i => new Power655PredictionDto
                    {
                        Id = Guid.NewGuid(),
                        TargetDrawDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i)),
                        PredictionType = "Frequency",
                        Numbers = [1, 7, 14, 21, 28, 35],
                        TupleFrequency = 3,
                        Reasoning = "Demo prediction",
                        MatchedNumbers = 0,
                        IsCompleted = false
                    }).ToList()
                });
            });
        }

        return DemoEnvelope.Success();
    }

}
