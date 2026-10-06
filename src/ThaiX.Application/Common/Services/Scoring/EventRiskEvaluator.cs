using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;

namespace ThaiX.Application.Common.Services.Scoring;

public sealed class EventRiskEvaluator : IEventRiskEvaluator
{
    // Cap cumulative penalty so a stock with many minor events is not worse than one with "halt"
    private const int MaxCumulativePenalty = -40;

    public EventRiskResult Evaluate(IReadOnlyList<VnDirectEventDto> events, int lookbackDays)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(-lookbackDays);

        var filteredEvents = events
            .Select(@event => new
            {
                Event = @event,
                Date = GetEventDate(@event),
                Penalty = GetEventPenalty(@event.Type),
                Level = GetEventRiskLevel(@event.Type)
            })
            .Where(x => x.Date is { } date && date <= now && date >= cutoff)
            .ToList();

        if (!filteredEvents.Any())
        {
            return new EventRiskResult
            {
                Level = "None",
                Penalty = 0,
                MostSevereType = string.Empty,
                LatestEffectiveDate = string.Empty,
                Count = 0
            };
        }

        // OrderBy ascending on negative penalties → most negative (worst) comes first
        var mostSevere = filteredEvents
            .OrderBy(x => x.Penalty)
            .ThenByDescending(x => x.Date)
            .First();

        var latestDate = filteredEvents
            .Select(x => x.Date)
            .Where(d => d != DateTime.MinValue)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();

        // Cumulative penalty: sum all events, capped so stack of minor events
        // cannot exceed the cap (e.g., 5× "nomargin" = -50 → capped to -40)
        var totalPenalty = Math.Max(MaxCumulativePenalty, filteredEvents.Sum(x => x.Penalty));

        return new EventRiskResult
        {
            Level = mostSevere.Level,
            Penalty = totalPenalty,
            MostSevereType = mostSevere.Event.Type ?? string.Empty,
            LatestEffectiveDate = latestDate == DateTime.MinValue ? string.Empty : latestDate.ToString("yyyy-MM-dd"),
            Count = filteredEvents.Count
        };
    }

    private static int GetEventPenalty(string? eventType)
        => eventType?.Trim().ToLowerInvariant() switch
        {
            "halt" => -25,
            "suspend" => -20,
            "control" => -20,
            "alert" => -18,
            "nomargin" => -10,
            "noticed" => -4,
            _ => 0
        };

    private static string GetEventRiskLevel(string? eventType)
        => eventType?.Trim().ToLowerInvariant() switch
        {
            "halt" => "Severe",
            "suspend" => "Severe",
            "control" => "Severe",
            "alert" => "Severe",
            "nomargin" => "Medium",
            "noticed" => "Light",
            _ => "None"
        };

    private static DateTime GetEventDate(VnDirectEventDto @event)
    {
        if (TryParseDate(@event.EffectiveDate, out var effectiveDate)) return effectiveDate;
        if (TryParseDate(@event.DisclosureDate, out var disclosureDate)) return disclosureDate;
        return DateTime.MinValue;
    }

    private static bool TryParseDate(string? dateText, out DateTime result)
    {
        if (string.IsNullOrWhiteSpace(dateText)) { result = DateTime.MinValue; return false; }
        return DateTime.TryParse(dateText, out result);
    }
}
