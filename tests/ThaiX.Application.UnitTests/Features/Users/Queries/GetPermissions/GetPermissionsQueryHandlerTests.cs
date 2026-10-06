using ThaiX.Application.Features.Users.Queries.GetPermissions;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.UnitTests.Features.Users.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandlerTests
{
    private readonly GetPermissionsQueryHandler _sut = new();

    [Fact]
    public async Task Handle_ShouldReturnAllDomainPermissions()
    {
        var query = new GetPermissionsQuery();

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        var expected = Permissions.GetAll();
        result.Should().BeEquivalentTo(expected, opts => opts.WithStrictOrdering());
        result.Should().HaveCount(expected.Count);
    }

    [Fact]
    public async Task Handle_ShouldContainKnownPermissions()
    {
        var query = new GetPermissionsQuery();

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().Contain(Permissions.UserRead);
        result.Should().Contain(Permissions.UserWrite);
        result.Should().Contain(Permissions.ContactRead);
        result.Should().Contain(Permissions.MasterDataRead);
        result.Should().Contain(Permissions.SystemAdmin);
    }

    [Fact]
    public async Task Handle_ShouldReturnNonEmptyCollection()
    {
        var query = new GetPermissionsQuery();

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().NotBeEmpty();
    }
}
