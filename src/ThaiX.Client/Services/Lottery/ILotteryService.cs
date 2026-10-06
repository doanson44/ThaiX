using ThaiX.Client.Models.Lottery;

namespace ThaiX.Client.Services.Lottery;

/// <summary>
/// Client service for the Power 6/55 lottery analysis API.
/// </summary>
public interface ILotteryService
{
    Task<Power655AnalysisResponse> GetAnalysisAsync(CancellationToken cancellationToken = default);

    Task TriggerSyncAsync(CancellationToken cancellationToken = default);

    Task TriggerPredictAsync(CancellationToken cancellationToken = default);
}
