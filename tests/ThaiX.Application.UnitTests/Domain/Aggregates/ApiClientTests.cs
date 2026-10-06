using ThaiX.Domain.Aggregates.ApiClient;

namespace ThaiX.Application.UnitTests.Domain.Aggregates;

public sealed class ApiClientTests
{
    [Fact]
    public void Create_WithValidInput_ShouldCreateApiClient()
    {
        // Arrange
        var clientId = "test-client";
        var secretHash = "hashed-secret";
        var name = "Test Client";
        var description = "A test client";
        var scopes = new[] { "api.read", "api.write" };
        var createdBy = Guid.NewGuid();

        // Act
        var client = ApiClient.Create(clientId, secretHash, name, description, scopes, createdBy);

        // Assert
        client.Id.Should().NotBe(Guid.Empty);
        client.ClientId.Should().Be("test-client");
        client.ClientSecretHash.Should().Be("hashed-secret");
        client.Name.Should().Be("Test Client");
        client.Description.Should().Be("A test client");
        client.IsActive.Should().BeTrue();
        client.Scopes.Should().BeEquivalentTo(new[] { "api.read", "api.write" });
        client.CreatedBy.Should().Be(createdBy);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSetDescriptionNull()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        client.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidClientId_ShouldThrow(string? clientId)
    {
        var act = () => ApiClient.Create(clientId!, "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("clientId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidSecretHash_ShouldThrow(string? secretHash)
    {
        var act = () => ApiClient.Create("client", secretHash!, "Name", null, Array.Empty<string>(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("clientSecretHash");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrow(string? name)
    {
        var act = () => ApiClient.Create("client", "hash", name!, null, Array.Empty<string>(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("name");
    }

    [Fact]
    public void Create_WithEmptyCreatedBy_ShouldThrow()
    {
        var act = () => ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.Empty);

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("createdBy");
    }

    [Fact]
    public void UpdateScopes_ShouldReplaceExistingScopes()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, new[] { "old.scope" }, Guid.NewGuid());

        client.UpdateScopes(new[] { "new.scope1", "new.scope2" });

        client.Scopes.Should().BeEquivalentTo(new[] { "new.scope1", "new.scope2" });
    }

    [Fact]
    public void UpdateScopes_ShouldFilterWhitespaceAndDeduplicate()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        client.UpdateScopes(new[] { "scope1", "", "  ", "scope1", "scope2" });

        client.Scopes.Should().BeEquivalentTo(new[] { "scope1", "scope2" });
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());
        client.Deactivate();

        client.Activate();

        client.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        client.Deactivate();

        client.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_WithValidInput_ShouldUpdateNameAndDescription()
    {
        var client = ApiClient.Create("client", "hash", "Old Name", "Old Desc", Array.Empty<string>(), Guid.NewGuid());

        client.UpdateDetails("New Name", "New Description");

        client.Name.Should().Be("New Name");
        client.Description.Should().Be("New Description");
    }

    [Fact]
    public void UpdateDetails_WithNullDescription_ShouldClearDescription()
    {
        var client = ApiClient.Create("client", "hash", "Name", "Has Desc", Array.Empty<string>(), Guid.NewGuid());

        client.UpdateDetails("Name", null);

        client.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void UpdateDetails_WithInvalidName_ShouldThrow(string? name)
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        var act = () => client.UpdateDetails(name!, null);

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("name");
    }

    [Fact]
    public void UpdateSecret_WithValidHash_ShouldUpdate()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        client.UpdateSecret("new-hash");

        client.ClientSecretHash.Should().Be("new-hash");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void UpdateSecret_WithInvalidHash_ShouldThrow(string? hash)
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        var act = () => client.UpdateSecret(hash!);

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("newSecretHash");
    }

    [Fact]
    public void DeleteClient_ShouldSoftDelete()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        client.DeleteClient();

        client.IsDeleted.Should().BeTrue();
        client.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void DeleteClient_WhenAlreadyDeleted_ShouldNotChangeState()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());
        client.DeleteClient();
        var firstDeletedAt = client.DeletedAt;

        client.DeleteClient();

        client.DeletedAt.Should().Be(firstDeletedAt);
    }

    [Fact]
    public void RestoreClient_ShouldUndoSoftDelete()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());
        client.DeleteClient();

        client.RestoreClient();

        client.IsDeleted.Should().BeFalse();
        client.DeletedAt.Should().BeNull();
        client.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void RestoreClient_WhenNotDeleted_ShouldNotChangeState()
    {
        var client = ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());

        client.RestoreClient();

        client.IsDeleted.Should().BeFalse();
    }
}
