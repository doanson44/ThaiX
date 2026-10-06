using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Users.Queries.GetUserPermissions;

namespace ThaiX.Application.UnitTests.Features.Users.Queries.GetUserPermissions;

public sealed class GetUserPermissionsQueryHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly GetUserPermissionsQueryHandler _sut;

    public GetUserPermissionsQueryHandlerTests()
    {
        _sut = new GetUserPermissionsQueryHandler(_identityUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPermissionsFromService()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserPermissionsQuery(userId);
        var expectedPermissions = new List<string> { "User.Read", "User.Write" };

        _identityUserServiceMock
            .Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPermissions);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedPermissions, opts => opts.WithStrictOrdering());
        result.Should().HaveCount(2);
        result.Should().Contain("User.Read");
        result.Should().Contain("User.Write");
    }

    [Fact]
    public async Task Handle_WhenUserHasNoPermissions_ShouldReturnEmptyCollection()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserPermissionsQuery(userId);

        _identityUserServiceMock
            .Setup(x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassUserIdAndCancellationTokenToService()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserPermissionsQuery(userId);
        var cts = new CancellationTokenSource();

        _identityUserServiceMock
            .Setup(x => x.GetUserPermissionsAsync(userId, cts.Token))
            .ReturnsAsync(new List<string> { "User.Read" });

        await _sut.Handle(query, cts.Token);

        _identityUserServiceMock.Verify(
            x => x.GetUserPermissionsAsync(userId, cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentUserId_ShouldCallServiceWithCorrectUserId()
    {
        var userId = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001");
        var query = new GetUserPermissionsQuery(userId);

        _identityUserServiceMock
            .Setup(x => x.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        await _sut.Handle(query, CancellationToken.None);

        _identityUserServiceMock.Verify(
            x => x.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
