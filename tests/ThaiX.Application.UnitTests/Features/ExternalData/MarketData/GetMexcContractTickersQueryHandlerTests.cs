using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickers;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.ChainBroker;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData;

public sealed class GetMexcContractTickersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenApiReturnsValidData_ReturnsSuccessResponse()
    {
        var projects = new[]
        {
            ChainBrokerProject.Create(new ChainBrokerProjectSnapshot
            {
                Slug = "bitcoin",
                Ticker = "BTC",
                BrokerScore = "8.0",
                SecurityScore = "85",
                TwitterScore = 12,
                Rank = 100,
                TotalRaise = "5000000",
                PercentCirculating = "75",
                MarketCap = "200000000",
                Fdmc = "300000000",
                PublicRoi = "1.2",
                Tags = [],
                Blockchains = [],
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
                NextUnlock = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)).ToString("yyyy-MM-dd"),
                Percent = "6"
            })
        };

        var dbContext = new Mock<IApplicationDbContext>();
        dbContext.Setup(x => x.ChainBrokerProjects).Returns(TestDbSetHelpers.CreateMockDbSet(projects).Object);
        dbContext.Setup(x => x.ChainBrokerUnlocks).Returns(TestDbSetHelpers.CreateMockDbSet(unlocks).Object);

        var handler = new GetMexcContractTickersQueryHandler(new StubExternalDataService(), dbContext.Object);

        var result = await handler.Handle(new GetMexcContractTickersQuery(), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data.Single().Symbol.Should().Be("BTC_USDT");
        result.Data.Single().CompositeScore.Should().BeGreaterThan(0);
    }
}
