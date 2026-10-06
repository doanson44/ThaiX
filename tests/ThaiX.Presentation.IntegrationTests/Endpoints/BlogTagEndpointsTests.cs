using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.Blog.Tags;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class BlogTagEndpointsTests : IntegrationTestBase
{
    public BlogTagEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize paged API response.");
    }

    [Fact]
    public async Task SearchTags_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/blog/tags");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTag_WithoutBlogWritePermission_ShouldReturnForbidden()
    {
        var email = $"blogtag-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PostAsJsonAsync("/api/blog/tags", new { Name = $"tag-{Guid.NewGuid():N}" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTag_ThenSearch_ShouldFindIt()
    {
        await CreateAndAuthenticateAdminAsync();
        var name = $"UniqueTag{Guid.NewGuid():N}";

        var createResponse = await Client.PostAsJsonAsync("/api/blog/tags", new { Name = name });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var searchResponse = await Client.GetAsync($"/api/blog/tags?search={name}");
        var error = await searchResponse.Content.ReadAsStringAsync();
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK, error);
        var results = await ReadPagedResponseAsync<TagDto>(searchResponse);
        results.Data.Should().ContainSingle(t => t.Name == name);
    }

    [Fact]
    public async Task CreateTag_WithExistingName_ShouldReturnSameId()
    {
        await CreateAndAuthenticateAdminAsync();
        var name = $"DedupTag{Guid.NewGuid():N}";

        var first = await Client.PostAsJsonAsync("/api/blog/tags", new { Name = name });
        var firstId = (await ReadResponseAsync<Guid>(first)).Data;

        var second = await Client.PostAsJsonAsync("/api/blog/tags", new { Name = name });
        var secondId = (await ReadResponseAsync<Guid>(second)).Data;

        secondId.Should().Be(firstId);
    }

    [Fact]
    public async Task DeleteTag_WithoutBlogDeletePermission_ShouldReturnForbidden()
    {
        await CreateAndAuthenticateAdminAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/blog/tags", new { Name = $"del-noperm-{Guid.NewGuid():N}" });
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var email = $"blogtag-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead, Permissions.BlogWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.DeleteAsync($"/api/blog/tags/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTag_ShouldRemoveFromSearchResults()
    {
        await CreateAndAuthenticateAdminAsync();
        var name = $"ToDelete{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/tags", new { Name = name });
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var deleteResponse = await Client.DeleteAsync($"/api/blog/tags/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var searchResponse = await Client.GetAsync($"/api/blog/tags?search={name}");
        var results = await ReadPagedResponseAsync<TagDto>(searchResponse);
        results.Data.Should().NotContain(t => t.Id == id);
    }
}
