using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.ChainBroker;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData;

public sealed class GetMexcSpotTicker24HrQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenApiReturnsData_ReturnsSuccessResponse()
    {
        var projects = new[]
        {
            ChainBrokerProject.Create(new ChainBrokerProjectSnapshot
            {
                Slug = "bitcoin",
                Ticker = "BTC",
                BrokerScore = "7.5",
                SecurityScore = "90",
                TwitterScore = 10,
                Rank = 50,
                TotalRaise = "10000000",
                PercentCirculating = "80",
                MarketCap = "100000000",
                Fdmc = "200000000",
                PrivateRoi = "2",
                PublicRoi = "1.5",
                ListingDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"),
                Blockchains = [],
                Tags = [],
                Funds = [],
                Launchpads = []
            })
        };

        var unlocks = new[]
        {
            ChainBrokerUnlock.Create(new ChainBrokerUnlockSnapshot
            {
                Slug = "bitcoin-unlock",
                Ticker = "BTC",
                NextUnlock = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)).ToString("yyyy-MM-dd"),
                Percent = "3",
                RoundName = "Team"
            })
        };

        var dbContext = new Mock<IApplicationDbContext>();
        dbContext.Setup(x => x.ChainBrokerProjects)
            .Returns(TestDbSetHelpers.CreateMockDbSet(projects).Object);
        dbContext.Setup(x => x.ChainBrokerUnlocks)
            .Returns(TestDbSetHelpers.CreateMockDbSet(unlocks).Object);

        var handler = new GetMexcSpotTicker24HrQueryHandler(new StubExternalDataService(), dbContext.Object);

        var result = await handler.Handle(new GetMexcSpotTicker24HrQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Success");
        result.Data.Should().ContainSingle();
        result.Data.Single().Symbol.Should().Be("BTCUSDT");
        result.Data.Single().CompositeScore.Should().BeGreaterThan(0m);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNull_ReturnsFailureResponse()
    {
        var dbContext = new Mock<IApplicationDbContext>();
        dbContext.Setup(x => x.ChainBrokerProjects).Returns(TestDbSetHelpers.CreateMockDbSet(Array.Empty<ChainBrokerProject>()).Object);
        dbContext.Setup(x => x.ChainBrokerUnlocks).Returns(TestDbSetHelpers.CreateMockDbSet(Array.Empty<ChainBrokerUnlock>()).Object);

        var service = new StubExternalDataService { ShouldReturnNullForMexcSpotTicker24Hr = true };
        var handler = new GetMexcSpotTicker24HrQueryHandler(service, dbContext.Object);

        var result = await handler.Handle(new GetMexcSpotTicker24HrQuery(), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Data.Should().BeEmpty();
        result.Message.Should().Contain("Failed to retrieve");
    }

}
