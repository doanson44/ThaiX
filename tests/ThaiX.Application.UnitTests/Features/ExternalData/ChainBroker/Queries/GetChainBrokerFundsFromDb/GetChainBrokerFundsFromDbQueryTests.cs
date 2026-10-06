using ThaiX.Application.Common.Caching;
using ThaiX.Application.Features.ExternalData.ChainBroker.Queries.GetChainBrokerFundsFromDb;

namespace ThaiX.Application.UnitTests.Features.ExternalData.ChainBroker.Queries.GetChainBrokerFundsFromDb;

public sealed class GetChainBrokerFundsFromDbQueryTests
{
    [Fact]
    public void Query_Defaults_ShouldMatchExpectedValues()
    {
        // Arrange
        var query = new GetChainBrokerFundsFromDbQuery();

        // Assert
        query.PageNumber.Should().Be(1);
        query.PageSize.Should().Be(50);
        query.Search.Should().BeNull();
        query.CacheGroup.Should().Be(CacheGroups.ChainBrokerData);
        query.Expiration.Should().Be(TimeSpan.FromHours(6));
        query.IsVersionedList.Should().BeFalse();
    }

    [Fact]
    public void Query_WithSearchAndPaging_ShouldBuildStableCacheKey()
    {
        // Arrange
        var query1 = new GetChainBrokerFundsFromDbQuery
        {
            Search = "abc",
            PageNumber = 2,
            PageSize = 25
        };

        var query2 = new GetChainBrokerFundsFromDbQuery
        {
            Search = "abc",
            PageNumber = 2,
            PageSize = 25
        };

        // Assert
        query1.CacheKey.Should().Be(query2.CacheKey);
        query1.CacheKey.Should().NotBeNullOrWhiteSpace();
    }
}
