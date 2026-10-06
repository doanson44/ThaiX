using ThaiX.Application.Common.Models;
using ThaiX.Application.Common.Validators;

namespace ThaiX.Application.UnitTests.Common.Validators;

public sealed class PagedRequestValidatorTests
{
    private readonly PagedRequestValidator _sut = new();

    [Fact]
    public void Validate_WithDefaults_ShouldBeValid()
    {
        var request = new PagedRequest();

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_WithValidPageNumber_ShouldBeValid(int pageNumber)
    {
        var request = new PagedRequest { PageNumber = pageNumber };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidPageNumber_ShouldHaveError(int pageNumber)
    {
        var request = new PagedRequest { PageNumber = pageNumber };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PagedRequest.PageNumber));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WithValidPageSize_ShouldBeValid(int pageSize)
    {
        var request = new PagedRequest { PageSize = pageSize };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithPageSizeTooSmall_ShouldHaveError(int pageSize)
    {
        var request = new PagedRequest { PageSize = pageSize };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PagedRequest.PageSize));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(1000)]
    public void Validate_WithPageSizeTooLarge_ShouldHaveError(int pageSize)
    {
        var request = new PagedRequest { PageSize = pageSize };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PagedRequest.PageSize));
    }

    [Fact]
    public void Validate_WithValidSortBy_ShouldBeValid()
    {
        var request = new PagedRequest { SortBy = "Email" };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSortByExceedingMaxLength_ShouldHaveError()
    {
        var request = new PagedRequest { SortBy = new string('x', 101) };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PagedRequest.SortBy));
    }

    [Fact]
    public void Validate_WithNullSortBy_ShouldBeValid()
    {
        var request = new PagedRequest { SortBy = null };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSortByAtMaxLength_ShouldBeValid()
    {
        var request = new PagedRequest { SortBy = new string('x', 100) };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
