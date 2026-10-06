using ThaiX.Application.Features.Users.Queries.GetUsers;

namespace ThaiX.Application.UnitTests.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryValidatorTests
{
    private readonly GetUsersQueryValidator _sut = new();

    [Fact]
    public void Validate_WithDefaults_ShouldNotHaveErrors()
    {
        var query = new GetUsersQuery();

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidSearchTerm_ShouldNotHaveErrors()
    {
        var query = new GetUsersQuery
        {
            SearchTerm = "john"
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSearchTermExceedingMaxLength_ShouldHaveError()
    {
        var query = new GetUsersQuery
        {
            SearchTerm = new string('a', 101)
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithSearchTermAtMaxLength_ShouldNotHaveErrors()
    {
        var query = new GetUsersQuery
        {
            SearchTerm = new string('a', 100)
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidPagination_ShouldNotHaveErrors()
    {
        var query = new GetUsersQuery
        {
            PageNumber = 2,
            PageSize = 20
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPageNumberLessThanOne_ShouldHaveError()
    {
        var query = new GetUsersQuery
        {
            PageNumber = 0
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithPageSizeLessThanOne_ShouldHaveError()
    {
        var query = new GetUsersQuery
        {
            PageSize = 0
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithPageSizeGreaterThanMax_ShouldHaveError()
    {
        var query = new GetUsersQuery
        {
            PageSize = 101
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithSortByExceedingMaxLength_ShouldHaveError()
    {
        var query = new GetUsersQuery
        {
            SortBy = new string('x', 101)
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithIsActiveFilter_ShouldNotHaveErrors()
    {
        var query = new GetUsersQuery
        {
            IsActive = true
        };

        var result = _sut.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
