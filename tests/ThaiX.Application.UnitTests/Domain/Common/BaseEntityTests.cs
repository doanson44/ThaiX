using ThaiX.Domain.Aggregates.ApiClient;

namespace ThaiX.Application.UnitTests.Domain.Common;

public sealed class BaseEntityTests
{
    [Fact]
    public void DomainEvents_Initially_ShouldBeEmpty()
    {
        var client = CreateTestClient();

        client.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var client = CreateTestClient();

        // DomainEvents start empty, clearing should not throw
        client.ClearDomainEvents();

        client.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SetUpdatedAt_ShouldSetTimestamp()
    {
        var client = CreateTestClient();
        var now = DateTime.UtcNow;

        client.SetUpdatedAt(now);

        client.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public void Id_ShouldBeSetOnCreation()
    {
        var client = CreateTestClient();

        client.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreatedAt_ShouldBeSetOnCreation()
    {
        var client = CreateTestClient();

        client.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    private static ApiClient CreateTestClient()
    {
        return ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());
    }
}

public sealed class BaseAuditableEntityTests
{
    [Fact]
    public void SetAuditFields_ShouldSetCreatedByWhenNull()
    {
        var client = CreateTestClient();
        var updater = Guid.NewGuid();

        // CreatedBy is already set by Create, so SetAuditFields should NOT overwrite it
        var originalCreatedBy = client.CreatedBy;
        client.SetAuditFields(Guid.NewGuid(), updater);

        client.CreatedBy.Should().Be(originalCreatedBy);
        client.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void SetAuditFields_ShouldNotOverwriteCreatedBy()
    {
        var client = CreateTestClient();
        var creator = Guid.NewGuid();
        var updater = Guid.NewGuid();

        // CreatedBy is already set in Create
        var originalCreatedBy = client.CreatedBy;
        client.SetAuditFields(creator, updater);

        // CreatedBy was already set in Create, so it should not change
        client.CreatedBy.Should().Be(originalCreatedBy);
        client.UpdatedBy.Should().Be(updater);
    }

    [Fact]
    public void SetDeletedBy_ShouldSetDeletedByField()
    {
        var client = CreateTestClient();
        var userId = Guid.NewGuid();

        client.SetDeletedBy(userId);

        client.DeletedBy.Should().Be(userId);
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        var client = CreateTestClient();

        client.DeleteClient();

        client.IsDeleted.Should().BeTrue();
        client.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Restore_ShouldClearDeleteFields()
    {
        var client = CreateTestClient();
        client.DeleteClient();
        client.SetDeletedBy(Guid.NewGuid());

        client.RestoreClient();

        client.IsDeleted.Should().BeFalse();
        client.DeletedAt.Should().BeNull();
        client.DeletedBy.Should().BeNull();
    }

    private static ApiClient CreateTestClient()
    {
        return ApiClient.Create("client", "hash", "Name", null, Array.Empty<string>(), Guid.NewGuid());
    }
}
