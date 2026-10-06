using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.Blog.Categories;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class BlogCategoryEndpointsTests : IntegrationTestBase
{
    public BlogCategoryEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private static object BuildCreateRequest(string slug, string name = "Tech") => new
    {
        Name = name,
        Slug = slug,
        Description = "Technology posts",
        Icon = (string?)null,
        Color = (string?)null
    };

    [Fact]
    public async Task GetCategories_IsAnonymous_ShouldReturnOk()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/blog/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<IReadOnlyList<CategoryDto>>(response);
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCategory_WithoutBlogWritePermission_ShouldReturnForbidden()
    {
        var email = $"blogcat-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PostAsJsonAsync("/api/blog/categories", BuildCreateRequest($"noperm-{Guid.NewGuid():N}"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCategory_WithValidRequest_ShouldReturnCreatedAndAppearInList()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"tech-{Guid.NewGuid():N}";

        var response = await Client.PostAsJsonAsync("/api/blog/categories", BuildCreateRequest(slug));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await Client.GetAsync("/api/blog/categories");
        var list = await ReadResponseAsync<IReadOnlyList<CategoryDto>>(listResponse);
        list.Data.Should().Contain(c => c.Slug == slug);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ShouldReturnConflict()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"dup-cat-{Guid.NewGuid():N}";
        await Client.PostAsJsonAsync("/api/blog/categories", BuildCreateRequest(slug));

        var response = await Client.PostAsJsonAsync("/api/blog/categories", BuildCreateRequest(slug));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateCategory_ShouldModifyName()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"update-cat-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/categories", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var response = await Client.PutAsJsonAsync($"/api/blog/categories/{id}", new
        {
            Name = "Updated Name",
            Slug = slug,
            Description = (string?)null,
            Icon = (string?)null,
            Color = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await ReadResponseAsync<IReadOnlyList<CategoryDto>>(await Client.GetAsync("/api/blog/categories"));
        list.Data.Should().Contain(c => c.Name == "Updated Name");
    }

    [Fact]
    public async Task DeleteCategory_WithoutBlogDeletePermission_ShouldReturnForbidden()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"del-cat-noperm-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/categories", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var email = $"blogcat-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead, Permissions.BlogWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.DeleteAsync($"/api/blog/categories/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCategory_ShouldRemoveFromList()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"del-cat-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/categories", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var response = await Client.DeleteAsync($"/api/blog/categories/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await ReadResponseAsync<IReadOnlyList<CategoryDto>>(await Client.GetAsync("/api/blog/categories"));
        list.Data.Should().NotContain(c => c.Id == id);
    }
}
