using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractSnapshots;
using ThaiX.Application.UnitTests.Common;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData.Queries.GetMexcContractSnapshots;

public sealed class GetMexcContractSnapshotsQueryHandlerTests
{
    private sealed class TrackingExternalDataService : StubExternalDataService
    {
        public bool LastBypassCache { get; private set; }
        public int CallCount { get; private set; }

        public override Task<T?> GetMexcContractTickerAsync<T>(bool bypassCache = false, CancellationToken cancellationToken = default) where T : default
        {
            LastBypassCache = bypassCache;
            CallCount++;
            return base.GetMexcContractTickerAsync<T>(bypassCache, cancellationToken);
        }
    }

    [Fact]
    public async Task Handle_WhenBypassCacheIsTrue_PassesBypassCacheToExternalDataService()
    {
        // Arrange
        var stub = new TrackingExternalDataService();
        var handler = new GetMexcContractSnapshotsQueryHandler(stub);
        var query = new GetMexcContractSnapshotsQuery { BypassCache = true };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        stub.CallCount.Should().Be(1);
        stub.LastBypassCache.Should().BeTrue();
        result.Should().ContainSingle();
        result[0].Symbol.Should().Be("BTC_USDT");
        result[0].LastPrice.Should().Be(50000m);
        result[0].FundingRate.Should().Be(0.0001m);
        result[0].Volume24h.Should().Be(1000m);
    }

    [Fact]
    public async Task Handle_WhenBypassCacheIsFalse_PassesFalseToExternalDataService()
    {
        // Arrange
        var stub = new TrackingExternalDataService();
        var handler = new GetMexcContractSnapshotsQueryHandler(stub);
        var query = new GetMexcContractSnapshotsQuery { BypassCache = false };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        stub.CallCount.Should().Be(1);
        stub.LastBypassCache.Should().BeFalse();
        result.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        var stub = new TrackingExternalDataService
        {
            ShouldReturnNullForMexcContractTicker = true
        };
        var handler = new GetMexcContractSnapshotsQueryHandler(stub);

        // Act
        var result = await handler.Handle(new GetMexcContractSnapshotsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
