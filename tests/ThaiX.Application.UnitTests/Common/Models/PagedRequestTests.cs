using ThaiX.Application.Common.Models;

namespace ThaiX.Application.UnitTests.Common.Models;

public sealed class PagedRequestTests
{
    [Fact]
    public void Defaults_ShouldBeCorrect()
    {
        var request = new PagedRequest();

        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(10);
        request.SortBy.Should().BeNull();
        request.SortDescending.Should().BeFalse();
    }

    [Fact]
    public void ValidatedPageNumber_WithValidValue_ShouldReturnSame()
    {
        var request = new PagedRequest { PageNumber = 5 };

        request.ValidatedPageNumber.Should().Be(5);
    }

    [Fact]
    public void ValidatedPageNumber_WithZero_ShouldReturnOne()
    {
        var request = new PagedRequest { PageNumber = 0 };

        request.ValidatedPageNumber.Should().Be(1);
    }

    [Fact]
    public void ValidatedPageNumber_WithNegative_ShouldReturnOne()
    {
        var request = new PagedRequest { PageNumber = -5 };

        request.ValidatedPageNumber.Should().Be(1);
    }

    [Fact]
    public void ValidatedPageSize_WithValidValue_ShouldReturnSame()
    {
        var request = new PagedRequest { PageSize = 50 };

        request.ValidatedPageSize.Should().Be(50);
    }

    [Fact]
    public void ValidatedPageSize_WithZero_ShouldReturnMin()
    {
        var request = new PagedRequest { PageSize = 0 };

        request.ValidatedPageSize.Should().Be(PagedRequest.MinPageSize);
    }

    [Fact]
    public void ValidatedPageSize_WithNegative_ShouldClampToMin()
    {
        var request = new PagedRequest { PageSize = -10 };

        request.ValidatedPageSize.Should().Be(PagedRequest.MinPageSize);
    }

    [Fact]
    public void ValidatedPageSize_WithExceedingMax_ShouldClampToMax()
    {
        var request = new PagedRequest { PageSize = 500 };

        request.ValidatedPageSize.Should().Be(PagedRequest.MaxPageSize);
    }

    [Fact]
    public void ValidatedPageSize_AtBoundaries_ShouldWork()
    {
        new PagedRequest { PageSize = 1 }.ValidatedPageSize.Should().Be(1);
        new PagedRequest { PageSize = 100 }.ValidatedPageSize.Should().Be(100);
    }

    [Fact]
    public void CalculateSkip_FirstPage_ShouldReturnZero()
    {
        var request = new PagedRequest { PageNumber = 1, PageSize = 10 };

        request.CalculateSkip().Should().Be(0);
    }

    [Fact]
    public void CalculateSkip_SecondPage_ShouldReturnPageSize()
    {
        var request = new PagedRequest { PageNumber = 2, PageSize = 10 };

        request.CalculateSkip().Should().Be(10);
    }

    [Fact]
    public void CalculateSkip_ThirdPageWith25Size_ShouldReturn50()
    {
        var request = new PagedRequest { PageNumber = 3, PageSize = 25 };

        request.CalculateSkip().Should().Be(50);
    }

    [Fact]
    public void Constants_ShouldBeCorrect()
    {
        PagedRequest.MinPageSize.Should().Be(1);
        PagedRequest.MaxPageSize.Should().Be(100);
    }

    [Fact]
    public void SortBy_ShouldBeSettable()
    {
        var request = new PagedRequest { SortBy = "Email" };

        request.SortBy.Should().Be("Email");
    }

    [Fact]
    public void SortDescending_ShouldBeSettable()
    {
        var request = new PagedRequest { SortDescending = true };

        request.SortDescending.Should().BeTrue();
    }
}
