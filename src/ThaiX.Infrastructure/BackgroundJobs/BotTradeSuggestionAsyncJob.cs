using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;
using ThaiX.Infrastructure.Services.BotCommands;

namespace ThaiX.Infrastructure.BackgroundJobs;

public sealed class BotTradeSuggestionAsyncJob
{
    private readonly IMediator _mediator;
    private readonly IBotAsyncExecutionStateStore _stateStore;
    private readonly ILogger<BotTradeSuggestionAsyncJob> _logger;

    public BotTradeSuggestionAsyncJob(
        IMediator mediator,
        IBotAsyncExecutionStateStore stateStore,
        ILogger<BotTradeSuggestionAsyncJob> logger)
    {
        _mediator = mediator;
        _stateStore = stateStore;
        _logger = logger;
    }

    public async Task RunAsync(
        string executionId,
        TradeSuggestionAsyncRequest request,
        CancellationToken cancellationToken)
    {
        _stateStore.SetRunning(executionId);

        try
        {
            var suggestion = await _mediator.Send(new GetTradeSuggestionQuery
            {
                Symbol = request.Symbol,
                MarketType = request.MarketType,
                Timeframe = request.Timeframe,
                UseLongTermTimeframe = request.UseLongTermTimeframe,
                MarketRegime = request.MarketRegime,
                EventRisk = request.EventRisk
            }, cancellationToken);

            var plainText =
                $"{request.Symbol} [{request.Timeframe}] => signal={suggestion.Signal}, trend={suggestion.Trend}, setup={suggestion.Setup}, confidence={suggestion.Confidence.ToString("0.##", CultureInfo.InvariantCulture)}.";

            _stateStore.SetCompleted(executionId, plainText);
            _logger.LogInformation(
                "Bot async signal execution completed. ExecutionId={ExecutionId}, Symbol={Symbol}, Timeframe={Timeframe}",
                executionId,
                request.Symbol,
                request.Timeframe);
        }
        catch (Exception ex)
        {
            _stateStore.SetFailed(executionId, ErrorCodes.INTERNAL_ERROR, ex.Message);
            _logger.LogError(
                ex,
                "Bot async signal execution failed. ExecutionId={ExecutionId}, Symbol={Symbol}, Timeframe={Timeframe}",
                executionId,
                request.Symbol,
                request.Timeframe);
            throw;
        }
    }
}
