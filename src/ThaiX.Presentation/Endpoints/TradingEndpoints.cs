using Hangfire;
using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Trading.Queries.EvaluateWeeklySuggestionPerformance;
using ThaiX.Application.Features.Trading.Queries.ExportWeeklySuggestionTemplate;
using ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;
using ThaiX.Application.Features.Trading.Queries.GetWeeklySuggestionHistory;
using ThaiX.Application.Trading;
using ThaiX.Domain.Aggregates.TradingSuggestions;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;
using ThaiX.Presentation.Resources;

namespace ThaiX.Presentation.Endpoints;

public static class TradingEndpoints
{
    public static void MapTradingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/trading")
            .WithTags("Trading");

        // GET /api/trading/suggestion?symbol=BTCUSDT&marketType=Crypto&timeframe=Min15
        // Optional: &marketRegime=RiskOn&eventRisk=Low
        group.MapGet("/suggestion", async (
            string symbol,
            MarketType marketType,
            string timeframe,
            bool? useLongTermTimeframe,
            MarketRegime? marketRegime,
            EventRisk? eventRisk,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTradeSuggestionQuery
            {
                Symbol = symbol,
                MarketType = marketType,
                Timeframe = timeframe,
                UseLongTermTimeframe = useLongTermTimeframe ?? false,
                MarketRegime = marketRegime ?? MarketRegime.RiskOn,
                EventRisk = eventRisk ?? EventRisk.Low
            };

            var result = await mediator.Send(query, cancellationToken);

            var response = ApiResponse<TradeSuggestionDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.TradingView)
        .WithName("GetTradeSuggestion")
        .WithDescription(
            "Get trading suggestion based on kline execution engine. " +
            "Required: symbol, marketType (Stock=1|Crypto=2), timeframe. " +
            "Optional: useLongTermTimeframe (Stock only, default false; when true timeframe must be Week1 or Month1), " +
            "Optional: marketRegime (Neutral=0|RiskOn=1|RiskOff=2, default RiskOn), " +
            "eventRisk (Low=0|Medium=1|High=2, default Low). " +
            "Crypto timeframes: Min1/Min5/Min15/Min30/Min60/Hour4/Hour8/Day1/Week1/Month1. " +
            "Data is cached for 2 minutes.");

        // POST /api/trading/verdict — decisive AI opinion (TRADE / NO TRADE) on an already-fetched
        // suggestion. Thin passthrough to IAiTextGenerationService, mirrors ResumeAiEndpoints.cs —
        // no MediatR/CQRS needed since this doesn't touch the database.
        group.MapPost("/verdict", async (
            GetTradeVerdictRequest request,
            IAiTextGenerationService ai,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(25));

            var systemPrompt = AiPromptResources.Get("Trading.Verdict.System").WithCurrentCultureInstruction();
            var prompt = AiPromptResources.Format(
                "Trading.Verdict.User",
                request.Symbol,
                request.MarketType,
                request.Timeframe,
                request.Trend,
                request.Momentum,
                request.Setup,
                request.Signal,
                request.Confidence.ToString("P0"),
                Fmt(request.EntryPrice),
                Fmt(request.StopLoss),
                Fmt(request.TakeProfit1),
                Fmt(request.TakeProfit2));

            var result = await ai.GenerateAsync(prompt, systemPrompt, cts.Token);

            var lines = result.Text.Trim().Split('\n', 2);
            var firstLine = lines[0].Trim().ToUpperInvariant();
            var verdict = firstLine.Contains("NO TRADE") ? "NO TRADE"
                : firstLine.Contains("TRADE") ? "TRADE"
                : "REVIEW";
            var reasoning = lines.Length > 1 ? lines[1].Trim() : result.Text.Trim();

            var response = ApiResponse<TradeVerdictResponse>.SuccessResult(new TradeVerdictResponse
            {
                Verdict = verdict,
                Reasoning = reasoning
            });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.TradingView)
        .WithName("GetTradeVerdict")
        .WithDescription("Decisive AI opinion (TRADE or NO TRADE) with reasoning, based on an already-fetched trade suggestion.");

        group.MapGet("/suggestion-history", async (
            SuggestionAssetClass? assetClass,
            int? page,
            int? pageSize,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetWeeklySuggestionHistoryQuery
            {
                AssetClass = assetClass,
                Page = page ?? 1,
                PageSize = pageSize ?? 20
            }, cancellationToken);

            var response = ApiResponse<WeeklySuggestionHistoryResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.TradingView)
        .WithName("GetWeeklySuggestionHistory")
        .WithDescription("Get persisted weekly suggestion history. Optional filters: assetClass, page, pageSize.");

        group.MapPost("/suggestion-history/evaluate", async (
            EvaluateSuggestionHistoryRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new EvaluateWeeklySuggestionPerformanceQuery
            {
                AssetClass = request.AssetClass,
                LookbackReports = request.LookbackReports ?? 12,
                Symbols = request.Symbols
            }, cancellationToken);

            var response = ApiResponse<EvaluateWeeklySuggestionPerformanceResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.TradingView)
        .WithName("EvaluateWeeklySuggestionHistory")
        .WithDescription(
            "Evaluate persisted suggestions by input symbols and optional current prices. " +
            "Returns performance from entry price to provided current price.");

        group.MapGet("/suggestion-history/export", async (
            string reportKey,
            SuggestionAssetClass assetClass,
            string? format,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var file = await mediator.Send(new ExportWeeklySuggestionTemplateQuery
            {
                ReportKey = reportKey,
                AssetClass = assetClass,
                Format = format ?? "csv"
            }, cancellationToken);

            return Results.File(file.FileContent, file.ContentType, file.FileName);
        })
        .RequireAuthorization(Permissions.TradingView)
        .WithName("ExportWeeklySuggestionTemplate")
        .WithDescription("Download weekly suggestion template file by reportKey. Supported formats: csv, md, markdown.");

        group.MapPost("/weekly-suggestion/run", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.TopStocksWeeklySuggestionJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted(null, response);
        })
        .RequireAuthorization(Permissions.TradingView)
        .WithName("TriggerTopStocksWeeklySuggestionJob")
        .WithDescription("Manually trigger TopStocksWeeklySuggestionJob. Returns Hangfire job ID.");

        group.MapPost("/weekly-suggestion/run-spot", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.MexcSpotWeeklySuggestionJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted(null, response);
        })
        .RequireAuthorization(Permissions.TradingView)
        .WithName("TriggerMexcSpotWeeklySuggestionJob")
        .WithDescription("Manually trigger MexcSpotWeeklySuggestionJob. Returns Hangfire job ID.");
    }

    public sealed record EvaluateSuggestionHistoryRequest
    {
        public SuggestionAssetClass? AssetClass { get; init; }
        public int? LookbackReports { get; init; }
        public required IReadOnlyList<EvaluateWeeklySuggestionSymbolInput> Symbols { get; init; }
    }

    private static string Fmt(decimal? value) => value?.ToString() ?? "n/a";
}

public sealed record GetTradeVerdictRequest
{
    public required string Symbol { get; init; }
    public required string MarketType { get; init; }
    public required string Timeframe { get; init; }
    public required string Trend { get; init; }
    public required string Momentum { get; init; }
    public required string Setup { get; init; }
    public required string Signal { get; init; }
    public decimal? EntryPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit1 { get; init; }
    public decimal? TakeProfit2 { get; init; }
    public decimal Confidence { get; init; }
}

public sealed record TradeVerdictResponse
{
    public string Verdict { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
}
