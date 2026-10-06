using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.UnitTests.Domain.Common;

public sealed class PermissionsTests
{
    [Fact]
    public void GetAll_ShouldReturnAllDefinedPermissions()
    {
        var all = Permissions.GetAll();

        all.Should().NotBeEmpty();
        all.Should().Contain(Permissions.UserRead);
        all.Should().Contain(Permissions.UserWrite);
        all.Should().Contain(Permissions.UserDelete);
        all.Should().Contain(Permissions.SystemAdmin);
        all.Should().Contain(Permissions.HangfireView);
        all.Should().Contain(Permissions.ApiClientRead);
    }

    [Fact]
    public void GetAll_ShouldReturnOnlyStringConstants()
    {
        var all = Permissions.GetAll();

        all.Should().AllSatisfy(p =>
        {
            p.Should().NotBeNullOrWhiteSpace();
            p.Should().Contain(".");
        });
    }

    [Fact]
    public void GetAdminPermissions_ShouldReturnAllPermissions()
    {
        var admin = Permissions.GetAdminPermissions();
        var all = Permissions.GetAll();

        admin.Should().BeEquivalentTo(all);
    }

    [Fact]
    public void PermissionConstants_ShouldFollowFeatureActionFormat()
    {
        Permissions.UserRead.Should().Be("User.Read");
        Permissions.UserWrite.Should().Be("User.Write");
        Permissions.RoleRead.Should().Be("Role.Read");
        Permissions.SystemAdmin.Should().Be("System.Admin");
        Permissions.HangfireView.Should().Be("Hangfire.View");
        Permissions.ApiClientRead.Should().Be("ApiClient.Read");
    }

    [Fact]
    public void GetAll_ShouldReturnReadOnlyCollection()
    {
        var all = Permissions.GetAll();

        all.Should().BeAssignableTo<IReadOnlyCollection<string>>();
    }
}
