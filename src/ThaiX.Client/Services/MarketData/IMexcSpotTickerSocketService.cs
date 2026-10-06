using ThaiX.Client.Models.ExternalData;

namespace ThaiX.Client.Services.MarketData;

public interface IMexcSpotTickerSocketService
{
    event Action<IReadOnlyCollection<MexcSpotTicker24HrDto>>? TickersUpdated;
    event Action<string>? StreamError;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}
