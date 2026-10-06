using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Lottery;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.Persistence;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that generates Pick3, Pick4, Pick5, and Pick6 predictions
/// for the next Power 6/55 draw date (Tuesday/Thursday/Saturday).
/// Uses N-tuple frequency analysis (same method as GetPower655AnalysisQueryHandler)
/// to feed AI with top candidate N-tuples, then AI selects and fills remaining numbers.
/// </summary>
[DisableConcurrentExecution(timeoutInSeconds: 0)]
public sealed class Power655PredictJob : HangfireJobBase
{
    private static readonly PredictionType[] PredictionTypes =
        [PredictionType.Pick3, PredictionType.Pick4, PredictionType.Pick5, PredictionType.Pick6];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAiTextGenerationService? _aiService;
    private readonly INotificationRouter _notificationRouter;
    private readonly ICacheService _cache;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<Power655PredictJob> _logger;

    public Power655PredictJob(
        IServiceScopeFactory scopeFactory,
        IAiTextGenerationService? aiService,
        INotificationRouter notificationRouter,
        ICacheService cache,
        IDateTimeProvider dateTimeProvider,
        IHangfireJobState hangfireJobState,
        ILogger<Power655PredictJob> logger) : base(hangfireJobState, logger)
    {
        _scopeFactory = scopeFactory;
        _aiService = aiService;
        _notificationRouter = notificationRouter;
        _cache = cache;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.Power655PredictJob))
        {
            return;
        }

        var started = DateTime.UtcNow;
        var targetDrawDate = GetNextDrawDate(_dateTimeProvider.LocalNow);

        try
        {
            // Skip if all prediction types already exist for this draw date
            if (await AllPredictionsExistAsync(targetDrawDate, cancellationToken))
            {
                _logger.LogInformation(
                    "Power655PredictJob: all predictions already exist for {DrawDate}, skipping",
                    targetDrawDate);
                return;
            }

            var officialDraws = await LoadOfficialDrawsAsync(cancellationToken);
            if (officialDraws.Count == 0)
            {
                _logger.LogWarning("Power655PredictJob: no official history found, skipping");
                return;
            }

            // Pre-compute individual frequency + N-tuple indexes for all types
            var individualFrequency = ComputeIndividualFrequency(officialDraws);
            var ntupleIndexes = new Dictionary<int, IReadOnlyList<NtupleCandidate>>();
            foreach (var type in PredictionTypes)
            {
                ntupleIndexes[(int)type] = BuildTopNCandidateTuples(officialDraws, (int)type);
            }

            var countsSaved = 0;
            var predictionLines = new List<string>();
            foreach (var type in PredictionTypes)
            {
                var (saved, line) = await GenerateAndSaveAsync(
                    targetDrawDate, type, officialDraws, individualFrequency, ntupleIndexes[(int)type], cancellationToken);
                if (saved)
                {
                    countsSaved++;
                }
                if (line is not null)
                {
                    predictionLines.Add(line);
                }
            }

            await _cache.InvalidateGroupAsync(CacheGroups.Lottery, cancellationToken);

            var elapsed = (int)(DateTime.UtcNow - started).TotalSeconds;

            var sections = new List<NotificationSection>();
            if (predictionLines.Count > 0)
            {
                sections.Add(new NotificationSection
                {
                    Title = $"Predicted numbers for {targetDrawDate:dd-MM-yyyy}",
                    Text = string.Join("\n", predictionLines)
                });
            }

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "Power 6/55 Prediction",
                    Type = NotificationCardType.Summary,
                    Severity = NotificationSeverity.Info,
                    Metrics =
                    [
                        new() { Label = "Status", Value = "Success" },
                        new() { Label = "DrawDate", Value = targetDrawDate.ToString("dd-MM-yyyy") },
                        new() { Label = "PredictionTypes", Value = $"{countsSaved}/4" },
                        new() { Label = "Duration", Value = $"{elapsed}s" }
                    ],
                    Sections = sections
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Power655PredictJob completed in {Elapsed}s for {DrawDate}: {Count} prediction types saved",
                elapsed,
                targetDrawDate,
                countsSaved);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Power655PredictJob failed");

            await _notificationRouter.DispatchAsync(new NotificationMessage
            {
                Kind = NotificationKind.DataSync,
                Card = new NotificationCard
                {
                    Category = "Data Sync",
                    Title = "Power 6/55 Prediction",
                    Type = NotificationCardType.Error,
                    Severity = NotificationSeverity.Error,
                    Metrics =
                    [
                        new() { Label = "Code", Value = "POWER655_PREDICT_FAILED" },
                        new() { Label = "Message", Value = ex.Message }
                    ]
                }
            }, CancellationToken.None);

            throw;
        }
    }

    // ── Data access ──────────────────────────────────────────────────────

    private async Task<List<Power655Result>> LoadOfficialDrawsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.Power655Results
            .AsNoTracking()
            .OrderByDescending(x => x.DrawDate)
            .ToListAsync(cancellationToken);
    }

    // ── Per-type prediction ──────────────────────────────────────────────

    private async Task<(bool Saved, string? Line)> GenerateAndSaveAsync(
        DateOnly drawDate,
        PredictionType predictionType,
        IReadOnlyList<Power655Result> officialDraws,
        IReadOnlyDictionary<int, int> individualFrequency,
        IReadOnlyList<NtupleCandidate> ntupleCandidates,
        CancellationToken cancellationToken)
    {
        if (await HasPredictionAsync(drawDate, predictionType, cancellationToken))
        {
            _logger.LogInformation(
                "Power655PredictJob: {Type} prediction already exists for {DrawDate}, skipping",
                predictionType,
                drawDate);
            return (false, null);
        }

        var result = await GeneratePredictionAsync(
            predictionType, officialDraws, individualFrequency, ntupleCandidates, cancellationToken);

        if (result.Numbers.Count != 6)
        {
            _logger.LogWarning(
                "Power655PredictJob: {Type} prediction produced only {Got} numbers (need 6), skipping",
                predictionType,
                result.Numbers.Count);
            return (false, null);
        }

        var reasoning = result.Reasoning ?? string.Empty;
        var frequency = result.TupleFrequency;
        var nums = result.Numbers;

        var entity = Power655Prediction.Create(
            targetDrawDate: drawDate,
            predictionType: predictionType,
            num1: nums[0],
            num2: nums[1],
            num3: nums[2],
            num4: nums[3],
            num5: nums[4],
            num6: nums[5],
            tupleFrequency: frequency,
            reasoning: reasoning);

        await SavePredictionAsync(entity, cancellationToken);

        var line = $"{predictionType}: [{string.Join(", ", nums)}]";
        return (true, line);
    }

    private async Task<PredictionResult> GeneratePredictionAsync(
        PredictionType predictionType,
        IReadOnlyList<Power655Result> officialDraws,
        IReadOnlyDictionary<int, int> individualFrequency,
        IReadOnlyList<NtupleCandidate> ntupleCandidates,
        CancellationToken cancellationToken)
    {
        var n = (int)predictionType;

        // Top N-tuple is always the fixed base for Pick3/Pick4/Pick5
        var topNtuple = ntupleCandidates.FirstOrDefault();
        var fixedNumbers = n < 6 && ntupleCandidates.Count > 0
            ? topNtuple.Numbers.ToList()
            : (IReadOnlyList<int>?)null;

        var tupleFrequency = ntupleCandidates.Count > 0 ? topNtuple.Frequency : 0;

        // Build hot+cold pools (20 most frequent + 20 least frequent unique numbers)
        var hotNumbers = individualFrequency
            .OrderByDescending(x => x.Value).ThenBy(x => x.Key).Take(20)
            .Select(x => (number: x.Key, frequency: x.Value)).ToList();

        var coldNumbers = individualFrequency
            .OrderBy(x => x.Value).ThenBy(x => x.Key).Take(20)
            .Select(x => (number: x.Key, frequency: x.Value)).ToList();

        // Try AI to fill remaining numbers
        if (_aiService is not null && ntupleCandidates.Count > 0)
        {
            var aiResult = await TryAiPredictionAsync(
                predictionType, officialDraws, fixedNumbers, hotNumbers, coldNumbers, tupleFrequency, cancellationToken);
            if (aiResult is not null && aiResult.Value.Numbers.Count >= n)
            {
                return aiResult.Value;
            }
        }

        // Fallback: fixed N-tuple + top hot remaining numbers
        if (fixedNumbers is not null)
        {
            var remaining = 6 - n;
            var existingSet = new HashSet<int>(fixedNumbers);
            var fillNumbers = hotNumbers
                .Select(x => x.number)
                .Where(x => !existingSet.Contains(x))
                .Take(remaining)
                .OrderBy(x => x)
                .ToList();

            var allNumbers = fixedNumbers.Concat(fillNumbers).OrderBy(x => x).ToList();
            return new PredictionResult(
                allNumbers,
                tupleFrequency,
                $"Top {n}-tuple (freq: {tupleFrequency}) + top {remaining} hot fill numbers (AI failed or not available).",
                fillNumbers);
        }

        // Pick6 fallback: top 6 hot numbers
        {
            var top6 = hotNumbers.Take(6).Select(x => x.number).OrderBy(x => x).ToList();
            return new PredictionResult(
                top6,
                0,
                "Top 6 most frequent numbers (AI failed or not available).");
        }
    }

    private async Task<PredictionResult?> TryAiPredictionAsync(
        PredictionType predictionType,
        IReadOnlyList<Power655Result> officialDraws,
        IReadOnlyList<int>? fixedNumbers,
        IReadOnlyList<(int number, int frequency)> hotNumbers,
        IReadOnlyList<(int number, int frequency)> coldNumbers,
        int tupleFrequency,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(25));

        var prompt = BuildAiPrompt(predictionType, officialDraws, fixedNumbers, hotNumbers, coldNumbers, tupleFrequency);
        var systemPrompt = BuildAiSystemPrompt(predictionType);

        try
        {
            var result = await _aiService!.GenerateAsync(prompt, systemPrompt, cts.Token);
            return ParseAiResponse(result.Text, predictionType, fixedNumbers, tupleFrequency);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Power655PredictJob: AI prediction failed for {Type}", predictionType);
            return null;
        }
    }

    // ── AI prompt builders ───────────────────────────────────────────────

    private static string BuildAiSystemPrompt(PredictionType type)
    {
        return type switch
        {
            PredictionType.Pick3 => "You are a lottery analysis AI. I will provide a FIXED 3-number combination (the most frequent triple in history) " +
                "together with 20 hottest + 20 coldest individual numbers (with frequencies). " +
                "Your task: select exactly 3 additional numbers from the hot/cold pools (numbers NOT already in the fixed triple) to complete a full 6-number set. " +
                "Return strict JSON only: {\"extra_numbers\":[n4,n5,n6],\"reasoning\":\"why you picked these fill numbers\"}. " +
                "All numbers must be unique, 1-55, sorted ascending. The fixed triple is already known — do NOT repeat it in your response.",
            PredictionType.Pick4 => "You are a lottery analysis AI. I will provide a FIXED 4-number combination (the most frequent quad in history) " +
                "together with 20 hottest + 20 coldest individual numbers (with frequencies). " +
                "Your task: select exactly 2 additional numbers from the hot/cold pools to complete a full 6-number set. " +
                "Return strict JSON only: {\"extra_numbers\":[n5,n6],\"reasoning\":\"...\"}. " +
                "All numbers must be unique, 1-55, sorted ascending.",
            PredictionType.Pick5 => "You are a lottery analysis AI. I will provide a FIXED 5-number combination (the most frequent pentuple in history) " +
                "together with 20 hottest + 20 coldest individual numbers (with frequencies). " +
                "Your task: select exactly 1 additional number from the hot/cold pools to complete a full 6-number set. " +
                "Return strict JSON only: {\"extra_numbers\":[n6],\"reasoning\":\"...\"}. " +
                "All numbers must be unique, 1-55, sorted ascending.",
            PredictionType.Pick6 => "You are a lottery analysis AI. I will provide 20 hottest + 20 coldest individual numbers (with frequencies) from Power 6/55 history. " +
                "Your task: select the best 6-number combination from these pools (or any numbers 1-55). " +
                "Return strict JSON only: {\"numbers\":[n1,n2,n3,n4,n5,n6],\"reasoning\":\"...\"}. " +
                "All numbers must be unique, 1-55, sorted ascending.",
            _ => "Return strict JSON only."
        };
    }

    private static string BuildAiPrompt(
        PredictionType predictionType,
        IReadOnlyList<Power655Result> officialDraws,
        IReadOnlyList<int>? fixedNumbers,
        IReadOnlyList<(int number, int frequency)> hotNumbers,
        IReadOnlyList<(int number, int frequency)> coldNumbers,
        int tupleFrequency)
    {
        var n = (int)predictionType;
        var sb = new StringBuilder();

        sb.AppendLine($"Power 6/55 {n}-Number Prediction");
        sb.AppendLine(new string('-', 50));
        sb.AppendLine();

        // Fixed N-tuple (for Pick3/Pick4/Pick5)
        if (fixedNumbers is not null)
        {
            sb.AppendLine($"FIXED {n}-tuple (top frequency: {tupleFrequency}x in {officialDraws.Count} draws):");
            sb.AppendLine($"  [{string.Join(", ", fixedNumbers)}]");
            sb.AppendLine();
        }

        // Hot numbers (20 most frequent)
        sb.AppendLine("20 HOTTEST numbers (most frequent):");
        foreach (var item in hotNumbers)
        {
            sb.AppendLine($"  {item.number:D2}: {item.frequency}x");
        }
        sb.AppendLine();

        // Cold numbers (20 least frequent)
        sb.AppendLine("20 COLDEST numbers (least frequent):");
        foreach (var item in coldNumbers)
        {
            sb.AppendLine($"  {item.number:D2}: {item.frequency}x");
        }
        sb.AppendLine();

        // Recent history (shorter)
        sb.AppendLine($"Recent draws (latest 10 of {officialDraws.Count} total):");
        foreach (var draw in officialDraws.Take(10))
        {
            var nums = new[] { draw.Num1, draw.Num2, draw.Num3, draw.Num4, draw.Num5, draw.Num6 }
                .OrderBy(x => x);
            sb.AppendLine($"  {draw.DrawDate:dd-MM-yyyy}: [{string.Join(", ", nums)}]");
        }
        sb.AppendLine();

        // Task
        if (fixedNumbers is not null)
        {
            var remaining = 6 - n;
            var fixedSet = new HashSet<int>(fixedNumbers);
            sb.AppendLine("YOUR TASK:");
            sb.AppendLine($"The {n}-tuple [{string.Join(", ", fixedNumbers)}] is already locked in.");
            sb.AppendLine($"Select exactly {remaining} additional number(s) from the hot/cold pools above");
            sb.AppendLine($"(must NOT be any of: {string.Join(", ", fixedSet)}).");
            sb.AppendLine($"Put them in 'extra_numbers' and explain your reasoning.");
        }
        else
        {
            sb.AppendLine("YOUR TASK:");
            sb.AppendLine("Select the best 6-number combination from the hot/cold pools above (or any 1-55).");
            sb.AppendLine("Put them in 'numbers' and explain your reasoning.");
        }

        return sb.ToString();
    }

    // ── AI response parsing ──────────────────────────────────────────────

    private static PredictionResult? ParseAiResponse(string raw, PredictionType type, IReadOnlyList<int>? fixedNumbers, int tupleFrequency)
    {
        var text = raw.Trim();
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var end = text.LastIndexOf("```", StringComparison.Ordinal);
            text = end > 3 ? text[3..end].Trim() : text[3..].Trim();
            if (text.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                text = text[4..].Trim();
            }
        }

        try
        {
            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd <= jsonStart)
            {
                return null;
            }

            var json = text[jsonStart..(jsonEnd + 1)];

            using var doc = JsonDocument.Parse(json);
            var reasoning = string.Empty;
            if (doc.RootElement.TryGetProperty("reasoning", out var reasonEl))
            {
                reasoning = reasonEl.GetString();
            }

            // For Pick3/4/5: AI returns extra_numbers, combine with fixedNumbers
            if (type != PredictionType.Pick6 && fixedNumbers is not null)
            {
                if (doc.RootElement.TryGetProperty("extra_numbers", out var extraEl))
                {
                    var extraNumbers = extraEl.EnumerateArray()
                        .Select(x => x.GetInt32())
                        .Where(n => n is >= 1 and <= 55 && !fixedNumbers.Contains(n))
                        .Distinct()
                        .Take(6 - (int)type)
                        .OrderBy(n => n)
                        .ToList();

                    var remaining = 6 - (int)type;
                    if (extraNumbers.Count >= remaining)
                    {
                        var allNumbers = fixedNumbers.Concat(extraNumbers).OrderBy(n => n).ToList();
                        return new PredictionResult(allNumbers, tupleFrequency, reasoning, extraNumbers);
                    }
                }
            }

            // Try numbers field (Pick6, or fallback for Pick3/4/5 if extra_numbers not present)
            if (doc.RootElement.TryGetProperty("numbers", out var numsEl))
            {
                var numbers = numsEl.EnumerateArray()
                    .Select(x => x.GetInt32())
                    .Where(n => n is >= 1 and <= 55)
                    .Distinct()
                    .Take(6)
                    .OrderBy(n => n)
                    .ToList();

                if (numbers.Count >= 6)
                {
                    return new PredictionResult(numbers, tupleFrequency, reasoning);
                }
            }
        }
        catch (JsonException)
        {
            // fall through to regex extraction
        }

        // Regex fallback: extract all valid numbers, take first 6 unique
        var allMatches = Regex.Matches(raw, @"\b([1-9]|[1-4]\d|5[0-5])\b")
            .Select(m => int.Parse(m.Value))
            .Distinct()
            .Take(6)
            .OrderBy(n => n)
            .ToList();

        if (allMatches.Count < 6)
        {
            return null;
        }

        return new PredictionResult(allMatches, 0, "Regex-extracted from AI response.");
    }

    // ── N-tuple frequency analysis (same logic as query handler) ──────────

    /// <summary>
    /// Builds the top N-tuple candidates from all historical draws.
    /// For Pick6 (N=6), each draw itself is the tuple.
    /// </summary>
    private static IReadOnlyList<NtupleCandidate> BuildTopNCandidateTuples(
        IReadOnlyList<Power655Result> draws,
        int n)
    {
        var frequency = new Dictionary<string, int>();

        foreach (var draw in draws)
        {
            var numbers = new[] { draw.Num1, draw.Num2, draw.Num3, draw.Num4, draw.Num5, draw.Num6 };

            foreach (var combo in EnumerateCombinations(numbers, n))
            {
                var key = string.Join(",", combo);
                frequency.TryGetValue(key, out var count);
                frequency[key] = count + 1;
            }
        }

        return frequency
            .Select(kv => new NtupleCandidate(kv.Key, kv.Value))
            .OrderByDescending(x => x.Frequency)
            .ToList();
    }

    /// <summary>
    /// Returns all combinations of <paramref name="k"/> elements from <paramref name="source"/>,
    /// each sorted ascending. Source must have 6 elements.
    /// </summary>
    private static IEnumerable<int[]> EnumerateCombinations(int[] source, int k)
    {
        var result = new int[k];

        IEnumerable<int[]> Comb(int start, int depth)
        {
            if (depth == k)
            {
                var copy = new int[k];
                Array.Copy(result, copy, k);
                Array.Sort(copy);
                yield return copy;
                yield break;
            }

            for (var i = start; i < source.Length; i++)
            {
                result[depth] = source[i];
                foreach (var c in Comb(i + 1, depth + 1))
                {
                    yield return c;
                }
            }
        }

        return Comb(0, 0);
    }

    // ── Individual frequency ──────────────────────────────────────────────

    private static Dictionary<int, int> ComputeIndividualFrequency(IReadOnlyList<Power655Result> draws)
    {
        var counts = new Dictionary<int, int>(55);
        foreach (var draw in draws)
        {
            foreach (var n in new[] { draw.Num1, draw.Num2, draw.Num3, draw.Num4, draw.Num5, draw.Num6 })
            {
                counts.TryGetValue(n, out var c);
                counts[n] = c + 1;
            }
        }

        return counts;
    }

    // ── Persistence ──────────────────────────────────────────────────────

    private async Task<bool> AllPredictionsExistAsync(
        DateOnly drawDate,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var count = await db.Power655Predictions
            .AsNoTracking()
            .CountAsync(x => x.TargetDrawDate == drawDate, cancellationToken);

        return count >= PredictionTypes.Length;
    }

    private async Task<bool> HasPredictionAsync(
        DateOnly drawDate,
        PredictionType predictionType,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.Power655Predictions
            .AsNoTracking()
            .AnyAsync(x => x.TargetDrawDate == drawDate && x.PredictionType == predictionType, cancellationToken);
    }

    private async Task SavePredictionAsync(
        Power655Prediction entity,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Power655Predictions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ── Draw date calculation ─────────────────────────────────────────────

    /// <summary>
    /// Power 6/55 draws on Tue, Thu, Sat. If today is a draw day and before 6 PM,
    /// returns today. Otherwise returns the next upcoming draw date.
    /// </summary>
    private static DateOnly GetNextDrawDate(DateTime localNow)
    {
        var today = DateOnly.FromDateTime(localNow);
        var time = TimeOnly.FromDateTime(localNow);
        var cutoff = new TimeOnly(18, 0);

        var isDrawDay = localNow.DayOfWeek is DayOfWeek.Tuesday or DayOfWeek.Thursday or DayOfWeek.Saturday;

        if (isDrawDay && time < cutoff)
        {
            return today;
        }

        var daysUntilNext = localNow.DayOfWeek switch
        {
            DayOfWeek.Monday => 1,    // → Tuesday
            DayOfWeek.Tuesday => 2,   // → Thursday
            DayOfWeek.Wednesday => 1, // → Thursday
            DayOfWeek.Thursday => 2,  // → Saturday
            DayOfWeek.Friday => 1,    // → Saturday
            DayOfWeek.Saturday => 3,  // → Tuesday (next week)
            DayOfWeek.Sunday => 2,    // → Tuesday
            _ => 1
        };

        return today.AddDays(daysUntilNext);
    }

    // ── Helper types ─────────────────────────────────────────────────────

    private readonly record struct NtupleCandidate(string Key, int Frequency)
    {
        public int[] Numbers => Key.Split(',').Select(int.Parse).ToArray();
    }

    private readonly record struct PredictionResult(
        IReadOnlyList<int> Numbers,
        int TupleFrequency,
        string? Reasoning = null,
        IReadOnlyList<int>? Extra = null)
    {
        public IReadOnlyList<int> AllNumbers =>
            Extra is { Count: > 0 } ? Numbers.Concat(Extra).ToList() : Numbers;
    }
}
