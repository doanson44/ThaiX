using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Features.PriceAlerts.Queries.GetPriceAlerts;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.UnitTests.Features.PriceAlerts.Queries.GetPriceAlerts;

public sealed class GetPriceAlertsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithEnabledFilter_ShouldReturnOnlyEnabledAlerts()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("GetPriceAlertsQueryHandlerTests_Filter")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        context.Set<PriceAlert>().Add(PriceAlert.Create("BTCUSDT", AssetType.CryptoSpot, AlertCondition.Above, 40000m, null, false));
        var disabled = PriceAlert.Create("ETHUSDT", AssetType.CryptoSpot, AlertCondition.Below, 2000m, null, false);
        disabled.Update(AlertCondition.Below, 2000m, null, false, false);
        context.Set<PriceAlert>().Add(disabled);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPriceAlertsQueryHandler(context);
        var query = new GetPriceAlertsQuery { IsEnabled = true, PageNumber = 1, PageSize = 10 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Symbol.Should().Be("BTCUSDT");
    }

    [Fact]
    public async Task Handle_WithSortByTargetPriceDescending_ShouldReturnSortedResults()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase("GetPriceAlertsQueryHandlerTests_Sort")
            .Options;

        await using var context = new TestApplicationDbContext(options);
        context.Set<PriceAlert>().Add(PriceAlert.Create("A1", AssetType.VnStock, AlertCondition.Below, 100m, null, false));
        context.Set<PriceAlert>().Add(PriceAlert.Create("A2", AssetType.VnStock, AlertCondition.Below, 200m, null, false));
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetPriceAlertsQueryHandler(context);
        var query = new GetPriceAlertsQuery { SortBy = "targetprice", SortDescending = true, PageNumber = 1, PageSize = 10 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Select(item => item.Symbol).Should().ContainInOrder(new[] { "A2", "A1" });
    }
}
