using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Users.Queries.GetUsers;

namespace ThaiX.Application.UnitTests.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly GetUsersQueryHandler _sut;

    public GetUsersQueryHandlerTests()
    {
        _sut = new GetUsersQueryHandler(_identityUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new GetUsersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "admin"
        };

        var expectedResult = new PagedResult<UserListItemDto>
        {
            Items = new List<UserListItemDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@thaix.com",
                    EmailConfirmed = true,
                    LockoutEnabled = false
                }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        _identityUserServiceMock
            .Setup(x => x.GetUsersAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchTerm,
                query.IsActive,
                query.SortBy,
                query.SortDescending,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items[0].Email.Should().Be("admin@thaix.com");
    }

    [Fact]
    public async Task Handle_WithNoResults_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var query = new GetUsersQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SearchTerm = "nonexistent"
        };

        var emptyResult = new PagedResult<UserListItemDto>
        {
            Items = new List<UserListItemDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _identityUserServiceMock
            .Setup(x => x.GetUsersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldPassAllParametersToService()
    {
        // Arrange
        var query = new GetUsersQuery
        {
            PageNumber = 2,
            PageSize = 25,
            SearchTerm = "test",
            IsActive = true,
            SortBy = "Email",
            SortDescending = true
        };

        _identityUserServiceMock
            .Setup(x => x.GetUsersAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<UserListItemDto>
            {
                Items = new List<UserListItemDto>(),
                TotalCount = 0,
                PageNumber = 2,
                PageSize = 25
            });

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _identityUserServiceMock.Verify(
            x => x.GetUsersAsync(2, 25, "test", true, "Email", true, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
