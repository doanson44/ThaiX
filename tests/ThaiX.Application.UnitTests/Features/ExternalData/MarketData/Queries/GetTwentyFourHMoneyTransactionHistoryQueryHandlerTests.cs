using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetTwentyFourHMoneyTransactionHistory;

namespace ThaiX.Application.UnitTests.Features.ExternalData.MarketData.Queries;

public sealed class GetTwentyFourHMoneyTransactionHistoryQueryHandlerTests
{
    [Fact]
    public void BuildPriceComparison_WhenAggregatesSpanMultiplePeriods_ReturnsCorrectMetrics()
    {
        // Arrange
        var anchor = new DateOnly(2026, 11, 16);
        PriceComparisonAggregate[] aggregates =
        [
            new(anchor, 110m, 2, 30),
            new(anchor, 90m, 1, 20),
            new(anchor, 100m, 1, 10),
            new(anchor.AddDays(-3), 120m, 3, 40),
            new(anchor.AddDays(-3), 80m, 2, 50),
            new(anchor.AddDays(-10), 130m, 1, 60),
            new(anchor.AddDays(-40), 70m, 1, 70),
            new(anchor.AddDays(-100), 140m, 1, 80),
            new(anchor.AddDays(-200), 60m, 1, 90),
            new(anchor.AddDays(-400), 150m, 1, 100)
        ];

        // Act
        var result = GetTwentyFourHMoneyTransactionHistoryQueryHandler.BuildPriceComparison(
            aggregates,
            anchor,
            100m);

        // Assert
        result.Should().HaveCount(6);

        var oneDay = result[0];
        oneDay.Period.Should().Be("1D");
        oneDay.TransactionCount.Should().Be(4);
        oneDay.TotalVolume.Should().Be(60);
        oneDay.BetterVolume.Should().Be(30);
        oneDay.WorseVolume.Should().Be(20);
        oneDay.EqualVolume.Should().Be(10);
        oneDay.AveragePrice.Should().Be(101.6667m);
        oneDay.Top3Highest.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new PriceVolumePointDto { Price = 110m, Volume = 30 });
        oneDay.Top3Lowest.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new PriceVolumePointDto { Price = 90m, Volume = 20 });

        var oneWeek = result[1];
        oneWeek.TransactionCount.Should().Be(9);
        oneWeek.TotalVolume.Should().Be(150);
        oneWeek.BetterVolume.Should().Be(70);
        oneWeek.WorseVolume.Should().Be(70);
        oneWeek.EqualVolume.Should().Be(10);
        oneWeek.Top3Highest.Select(point => point.Price).Should().Equal(120m, 110m);
        oneWeek.Top3Lowest.Select(point => point.Price).Should().Equal(80m, 90m);

        result[2].Period.Should().Be("1M");
        result[2].TransactionCount.Should().Be(10);
        result[2].TotalVolume.Should().Be(210);

        result[3].Period.Should().Be("3M");
        result[3].TransactionCount.Should().Be(11);
        result[3].TotalVolume.Should().Be(280);

        result[4].Period.Should().Be("6M");
        result[4].TransactionCount.Should().Be(12);
        result[4].TotalVolume.Should().Be(360);

        var oneYear = result[5];
        oneYear.Period.Should().Be("1Y");
        oneYear.TransactionCount.Should().Be(13);
        oneYear.TotalVolume.Should().Be(450);
        oneYear.BetterVolume.Should().Be(210);
        oneYear.WorseVolume.Should().Be(230);
        oneYear.EqualVolume.Should().Be(10);
    }

    [Fact]
    public void BuildPriceComparison_WhenNoAggregatesInPeriod_ReturnsZeroMetrics()
    {
        // Arrange
        var anchor = new DateOnly(2026, 11, 16);
        PriceComparisonAggregate[] aggregates =
        [
            new(anchor.AddYears(-2), 100m, 1, 10)
        ];

        // Act
        var result = GetTwentyFourHMoneyTransactionHistoryQueryHandler.BuildPriceComparison(
            aggregates,
            anchor,
            100m);

        // Assert
        result.Should().HaveCount(6);
        result.Should().OnlyContain(period =>
            period.TransactionCount == 0 &&
            period.TotalVolume == 0 &&
            period.BetterVolume == 0 &&
            period.WorseVolume == 0 &&
            period.EqualVolume == 0 &&
            period.AveragePrice == 0);
    }
}