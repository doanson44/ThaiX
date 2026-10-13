using ThaiX.Client.Models.Lottery;


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
