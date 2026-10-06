using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Lottery.Queries.GetPower655Analysis;

public sealed class GetPower655AnalysisQueryHandler
    : IRequestHandler<GetPower655AnalysisQuery, Power655AnalysisResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPower655AnalysisQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Power655AnalysisResponse> Handle(
        GetPower655AnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var draws = await _dbContext.Power655Results
            .AsNoTracking()
            .OrderByDescending(r => r.DrawDate)
            .Select(r => new Power655DrawDto
            {
                Id = r.Id,
                DrawDate = r.DrawDate,
                DayOfWeek = r.DayOfWeek,
                Numbers = new[] { r.Num1, r.Num2, r.Num3, r.Num4, r.Num5, r.Num6 },
                BonusNum = r.BonusNum,
                Jackpot1Value = r.Jackpot1Value,
                Jackpot2Value = r.Jackpot2Value
            })
            .ToListAsync(cancellationToken);

        var frequency = ComputeFrequency(draws);

        var predictions = await _dbContext.Power655Predictions
            .AsNoTracking()
            .OrderByDescending(p => p.TargetDrawDate)
            .ThenBy(p => p.PredictionType)
            .Select(p => new Power655PredictionDto
            {
                Id = p.Id,
                TargetDrawDate = p.TargetDrawDate,
                PredictionType = p.PredictionType.ToString(),
                Numbers = new[] { p.Num1, p.Num2, p.Num3, p.Num4, p.Num5, p.Num6 },
                TupleFrequency = p.TupleFrequency,
                Reasoning = p.Reasoning,
                MatchedNumbers = p.MatchedNumbers,
                IsCompleted = p.Power655ResultId != null
            })
            .ToListAsync(cancellationToken);

        return new Power655AnalysisResponse
        {
            Draws = draws,
            TopMostFrequent = frequency.TopMostFrequent,
            TopLeastFrequent = frequency.TopLeastFrequent,
            Predictions = predictions
        };
    }

    private static (IReadOnlyList<NumberFrequencyRecord> TopMostFrequent, IReadOnlyList<NumberFrequencyRecord> TopLeastFrequent)
        ComputeFrequency(IReadOnlyList<Power655DrawDto> draws)
    {
        var counts = new Dictionary<int, int>(55);
        foreach (var draw in draws)
        {
            foreach (var n in draw.Numbers)
            {
                counts.TryGetValue(n, out var c);
                counts[n] = c + 1;
            }
        }

        var sorted = counts
            .Select(kv => new NumberFrequencyRecord { Number = kv.Key, Frequency = kv.Value })
            .OrderByDescending(x => x.Frequency)
            .ThenBy(x => x.Number)
            .ToList();

        var topMost = sorted.Take(6).ToList();
        var topLeast = sorted.OrderBy(x => x.Frequency).ThenBy(x => x.Number).Take(6).ToList();

        return (topMost, topLeast);
    }
}
