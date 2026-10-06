using ThaiX.Client.Models.ExternalData;

namespace ThaiX.Client.Services.MarketData;

public interface IMexcContractTickerSocketService
{
    event Action<IReadOnlyCollection<MexcContractTickerDto>>? TickersUpdated;
    event Action<string>? StreamError;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync();
}
