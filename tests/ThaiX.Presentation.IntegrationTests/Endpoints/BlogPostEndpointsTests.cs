using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.Blog.Posts;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class BlogPostEndpointsTests : IntegrationTestBase
{
    public BlogPostEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private static object BuildCreateRequest(string slug, string title = "Hello Blog") => new
    {
        Title = title,
        Slug = slug,
        Summary = "A short summary of the post.",
        ContentHtml = "<h1>Heading</h1><p>Some <strong>content</strong> here.</p>",
        FeaturedImageUrl = (string?)null,
        CategoryId = (Guid?)null,
        MetaTitle = (string?)null,
        MetaDescription = (string?)null,
        TagIds = Array.Empty<Guid>()
    };

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize paged API response.");
    }

    #region GET /api/blog/posts

    [Fact]
    public async Task GetPosts_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/blog/posts");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPosts_WithoutBlogReadPermission_ShouldReturnForbidden()
    {
        var email = $"blog-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.GetAsync("/api/blog/posts");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPosts_WithBlogReadPermission_ShouldReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.GetAsync("/api/blog/posts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadPagedResponseAsync<PostListItemDto>(response);
        envelope.Success.Should().BeTrue();
    }

    #endregion

    #region POST /api/blog/posts

    [Fact]
    public async Task CreatePost_WithoutBlogWritePermission_ShouldReturnForbidden()
    {
        var email = $"blog-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest($"noperm-{Guid.NewGuid():N}"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePost_WithValidRequest_ShouldReturnCreatedAndPersist()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"valid-{Guid.NewGuid():N}";

        var response = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Data.Should().NotBeEmpty();

        var getResponse = await Client.GetAsync($"/api/blog/posts/{envelope.Data}");
        var post = await ReadResponseAsync<PostDto>(getResponse);
        post.Data!.Slug.Should().Be(slug);
        post.Data.Status.Should().Be(Domain.Aggregates.Blog.PostStatus.Draft);
    }

    [Fact]
    public async Task CreatePost_WithDuplicateSlug_ShouldReturnConflict()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"dup-{Guid.NewGuid():N}";
        await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));

        var response = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region PUT /api/blog/posts/{id}

    [Fact]
    public async Task UpdatePost_ShouldModifyFieldsAndReturnOk()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"update-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var updateRequest = new
        {
            Title = "Updated Title",
            Slug = slug,
            Summary = "Updated summary",
            ContentHtml = "Updated content",
            FeaturedImageUrl = (string?)null,
            CategoryId = (Guid?)null,
            MetaTitle = (string?)null,
            MetaDescription = (string?)null,
            TagIds = Array.Empty<Guid>()
        };

        var response = await Client.PutAsJsonAsync($"/api/blog/posts/{id}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/api/blog/posts/{id}");
        var post = await ReadResponseAsync<PostDto>(getResponse);
        post.Data!.Title.Should().Be("Updated Title");
    }

    #endregion

    #region Lifecycle: publish/schedule/unpublish/archive

    [Fact]
    public async Task PublishPost_WithoutBlogPublishPermission_ShouldReturnForbidden()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"publish-noperm-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var email = $"blog-publish-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead, Permissions.BlogWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PostAsync($"/api/blog/posts/{id}/publish", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublishPost_ShouldSetStatusPublishedAndAppearOnPublicEndpoint()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"publish-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var publishResponse = await Client.PostAsync($"/api/blog/posts/{id}/publish", content: null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization = null;
        var publicResponse = await Client.GetAsync($"/api/blog/posts/public/{slug}");
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var publicPost = await ReadResponseAsync<PostDto>(publicResponse);
        publicPost.Data!.Status.Should().Be(Domain.Aggregates.Blog.PostStatus.Published);
    }

    [Fact]
    public async Task GetPublicBySlug_WhenDraft_ShouldReturnNotFound()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"draft-public-{Guid.NewGuid():N}";
        await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));

        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/blog/posts/public/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SchedulePost_WithPastDate_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"schedule-past-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var response = await Client.PostAsJsonAsync($"/api/blog/posts/{id}/schedule", new { PublishAt = DateTime.UtcNow.AddDays(-1) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SchedulePost_WithFutureDate_ShouldSetStatusScheduled()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"schedule-future-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var response = await Client.PostAsJsonAsync($"/api/blog/posts/{id}/schedule", new { PublishAt = DateTime.UtcNow.AddDays(1) });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var post = await ReadResponseAsync<PostDto>(await Client.GetAsync($"/api/blog/posts/{id}"));
        post.Data!.Status.Should().Be(Domain.Aggregates.Blog.PostStatus.Scheduled);
    }

    [Fact]
    public async Task UnpublishPost_ShouldRevertToDraft()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"unpublish-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;
        await Client.PostAsync($"/api/blog/posts/{id}/publish", content: null);

        var response = await Client.PostAsync($"/api/blog/posts/{id}/unpublish", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var post = await ReadResponseAsync<PostDto>(await Client.GetAsync($"/api/blog/posts/{id}"));
        post.Data!.Status.Should().Be(Domain.Aggregates.Blog.PostStatus.Draft);
    }

    [Fact]
    public async Task ArchivePost_ShouldSetStatusArchived()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"archive-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var response = await Client.PostAsync($"/api/blog/posts/{id}/archive", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var post = await ReadResponseAsync<PostDto>(await Client.GetAsync($"/api/blog/posts/{id}"));
        post.Data!.Status.Should().Be(Domain.Aggregates.Blog.PostStatus.Archived);
    }

    #endregion

    #region DELETE / restore / duplicate

    [Fact]
    public async Task DeletePost_WithoutBlogDeletePermission_ShouldReturnForbidden()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"delete-noperm-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var email = $"blog-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead, Permissions.BlogWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.DeleteAsync($"/api/blog/posts/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletePost_ThenRestore_ShouldRoundTrip()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"delete-restore-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var deleteResponse = await Client.DeleteAsync($"/api/blog/posts/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterDelete = await Client.GetAsync($"/api/blog/posts/{id}");
        afterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var restoreResponse = await Client.PostAsync($"/api/blog/posts/{id}/restore", content: null);
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterRestore = await Client.GetAsync($"/api/blog/posts/{id}");
        afterRestore.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DuplicatePost_ShouldCreateNewDraftCopy()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"dup-source-{Guid.NewGuid():N}";
        var createResponse = await Client.PostAsJsonAsync("/api/blog/posts", BuildCreateRequest(slug, "Original Title"));
        var id = (await ReadResponseAsync<Guid>(createResponse)).Data;

        var response = await Client.PostAsync($"/api/blog/posts/{id}/duplicate", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var newId = (await ReadResponseAsync<Guid>(response)).Data;
        newId.Should().NotBe(id);

        var copy = await ReadResponseAsync<PostDto>(await Client.GetAsync($"/api/blog/posts/{newId}"));
        copy.Data!.Title.Should().Be("Original Title (Copy)");
        copy.Data.Status.Should().Be(Domain.Aggregates.Blog.PostStatus.Draft);
    }

    #endregion
}
