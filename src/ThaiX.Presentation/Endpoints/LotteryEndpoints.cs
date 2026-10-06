using Hangfire;
using MediatR;
using ThaiX.Application.Features.Lottery.Queries.GetPower655Analysis;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Lottery-related endpoints: Power 6/55 analysis with full draw list, frequency stats, and AI prediction.
/// </summary>
public static class LotteryEndpoints
{
    public static void MapLotteryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/lottery")
            .WithTags("Lottery");

        // GET /api/lottery/power-655/analysis
        group.MapGet("/power-655/analysis", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPower655AnalysisQuery(), cancellationToken);

            var response = ApiResponse<Power655AnalysisResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetPower655Analysis")
        .WithSummary("Get full Power 6/55 analysis")
        .WithDescription(
            "Analyze Power 6/55 lottery history from the database. " +
            "Returns the full draw history, top 6 most/least frequent numbers with counts, " +
            "and an optional AI prediction for the next draw. " +
            "This endpoint does not support server-side pagination.");

        // POST /api/lottery/power-655/sync
        group.MapPost("/power-655/sync", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.Power655SyncJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("TriggerPower655Sync")
        .WithDescription(
            "Manually trigger Power655SyncJob to sync Power 6/55 results from ketquadientoan.com. " +
            "Returns Hangfire job ID.");

        // POST /api/lottery/power-655/predict
        group.MapPost("/power-655/predict", (
            IBackgroundJobClient jobClient,
            HttpContext httpContext) =>
        {
            var jobId = jobClient.Enqueue<Infrastructure.BackgroundJobs.Power655PredictJob>(
                job => job.RunAsync(CancellationToken.None));

            var response = ApiResponse<object>.SuccessResult(new { jobId });
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("TriggerPower655Predict")
        .WithDescription(
            "Manually trigger Power655PredictJob to store prediction numbers for today's draw date. " +
            "If a prediction already exists for today, the job exits without changes. Returns Hangfire job ID.");
    }
}
