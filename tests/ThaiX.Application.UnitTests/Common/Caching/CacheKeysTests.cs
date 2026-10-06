using ThaiX.Application.Common.Caching;

namespace ThaiX.Application.UnitTests.Common.Caching;

public sealed class CacheKeysTests
{
    [Fact]
    public void PortfoliosList_ShouldIncludeSearchTermInCacheKey()
    {
        // Arrange
        var ownerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var keyWithoutSearch = CacheKeys.Portfolios.List(ownerId, null, page: 1, size: 20);
        var keyWithSearch = CacheKeys.Portfolios.List(ownerId, "Growth", page: 1, size: 20);
        var keyWithTrimmedSearch = CacheKeys.Portfolios.List(ownerId, "  growth  ", page: 1, size: 20);

        // Assert
        keyWithoutSearch.Should().NotBe(keyWithSearch);
        keyWithSearch.Should().Be(keyWithTrimmedSearch);
        keyWithSearch.Should().Contain("growth");
    }
}
