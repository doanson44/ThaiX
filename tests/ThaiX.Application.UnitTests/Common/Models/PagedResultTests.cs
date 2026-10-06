using ThaiX.Application.Common.Models;

namespace ThaiX.Application.UnitTests.Common.Models;

public sealed class PagedResultTests
{
    [Fact]
    public void TotalPages_WithExactFit_ShouldCalculateCorrectly()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new[] { "a", "b" },
            TotalCount = 20,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void TotalPages_WithRemainder_ShouldRoundUp()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new[] { "a" },
            TotalCount = 21,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_WithZeroPageSize_ShouldReturnZero()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = Array.Empty<string>(),
            TotalCount = 5,
            PageNumber = 1,
            PageSize = 0
        };

        // Assert
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void HasPrevious_OnFirstPage_ShouldBeFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new[] { "a" },
            TotalCount = 20,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_OnSecondPage_ShouldBeTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new[] { "a" },
            TotalCount = 20,
            PageNumber = 2,
            PageSize = 10
        };

        // Assert
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void HasNext_OnLastPage_ShouldBeFalse()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new[] { "a" },
            TotalCount = 20,
            PageNumber = 2,
            PageSize = 10
        };

        // Assert
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasNext_OnFirstPageWithMorePages_ShouldBeTrue()
    {
        // Arrange
        var result = new PagedResult<string>
        {
            Items = new[] { "a" },
            TotalCount = 20,
            PageNumber = 1,
            PageSize = 10
        };

        // Assert
        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public void Empty_ShouldReturnEmptyResult()
    {
        // Act
        var result = PagedResult<string>.Empty(pageNumber: 3, pageSize: 25);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(3);
        result.PageSize.Should().Be(25);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public void Empty_WithDefaults_ShouldReturnPageOneSize10()
    {
        // Act
        var result = PagedResult<int>.Empty();

        // Assert
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };

        // Act
        var result = PagedResult<string>.Create(items, totalCount: 30, pageNumber: 2, pageSize: 3);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(30);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(3);
        result.TotalPages.Should().Be(10);
        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeTrue();
    }
}
