using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.PriceAlerts.Queries.GetAllPriceAlerts;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.UnitTests.Features.PriceAlerts.Queries.GetAllPriceAlerts;

public sealed class GetAllPriceAlertsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyEnabledNonDeletedAlertsOrderedByAssetTypeAndSymbol()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("GetAllPriceAlertsQueryHandlerTests")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        var alert1 = PriceAlert.Create("BTCUSDT", AssetType.CryptoSpot, AlertCondition.Above, 40000m, null, false);
        var alert2 = PriceAlert.Create("AAPL", AssetType.VnStock, AlertCondition.Below, 150m, null, false);
        var alert3 = PriceAlert.Create("XRPUSDT", AssetType.CryptoSpot, AlertCondition.Above, 0.5m, null, false);
        alert3.Disable();
        context.Set<PriceAlert>().AddRange(alert1, alert2, alert3);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetAllPriceAlertsQueryHandler(context);
        var query = new GetAllPriceAlertsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(a => a.Symbol).Should().Equal("BTCUSDT", "AAPL");
    }
}
