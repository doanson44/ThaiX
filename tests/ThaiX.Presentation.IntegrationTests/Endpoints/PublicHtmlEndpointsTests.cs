using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class PublicHtmlEndpointsTests(ThaiXWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetPages_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/public-html/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPages_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"public-html-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/public-html/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Upload_WithInvalidExtension_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        using var form = CreateUploadForm($"invalid-{Guid.NewGuid():N}", "test.txt", "plain text");

        // Act
        var response = await Client.PostAsync("/api/public-html/", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithDuplicateSlug_ShouldReturnConflict()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var slug = $"duplicate-{Guid.NewGuid():N}";
        var firstResponse = await UploadAsync(slug);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var duplicateForm = CreateUploadForm(slug, "duplicate.html", "<html>duplicate</html>");

        // Act
        var response = await Client.PostAsync("/api/public-html/", duplicateForm);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var id = await ReadCreatedIdAsync(firstResponse);
        await Client.DeleteAsync($"/api/public-html/{id}");
    }

    [Fact]
    public async Task PublishedPage_WhenRequestedAnonymously_ShouldReturnSandboxedHtmlAndContent()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var slug = $"published-{Guid.NewGuid():N}";
        var id = await UploadAndPublishAsync(slug);

        // Act
        Client.DefaultRequestHeaders.Authorization = null;
        var pageResponse = await Client.GetAsync($"/html/{slug}");
        var contentResponse = await Client.GetAsync($"/api/public-html/{slug}/content");

        // Assert
        pageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var wrapper = await pageResponse.Content.ReadAsStringAsync();
        wrapper.Should().Contain("sandbox=\"allow-scripts allow-forms allow-popups allow-modals\"");
        wrapper.Should().Contain("referrerpolicy=\"no-referrer\"");
        contentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await contentResponse.Content.ReadAsStringAsync()).Should().Contain("published page content");

        await AuthenticateTokenForAdminAsync();
        await Client.DeleteAsync($"/api/public-html/{id}");
    }

    [Fact]
    public async Task UnpublishedPage_WhenRequestedAnonymously_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var slug = $"unpublished-{Guid.NewGuid():N}";
        var response = await UploadAsync(slug);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedIdAsync(response);

        // Act
        Client.DefaultRequestHeaders.Authorization = null;
        var publicResponse = await Client.GetAsync($"/html/{slug}");

        // Assert
        publicResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AuthenticateTokenForAdminAsync();
        await Client.DeleteAsync($"/api/public-html/{id}");
    }

    [Fact]
    public async Task RenamePublishedPage_ShouldKeepPreviousSlugAsAlias()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var oldSlug = $"old-{Guid.NewGuid():N}";
        var id = await UploadAndPublishAsync(oldSlug);
        var newSlug = $"new-{Guid.NewGuid():N}";

        // Act
        var renameResponse = await Client.PatchAsJsonAsync($"/api/public-html/{id}/slug", new { Slug = newSlug });

        // Assert
        renameResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        Client.DefaultRequestHeaders.Authorization = null;
        var oldSlugResponse = await Client.GetAsync($"/html/{oldSlug}");
        var newSlugResponse = await Client.GetAsync($"/html/{newSlug}");
        oldSlugResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        newSlugResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await AuthenticateTokenForAdminAsync();
        await Client.DeleteAsync($"/api/public-html/{id}");
    }

    [Fact]
    public async Task DeletePublishedPage_ShouldRemovePublicAccess()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var slug = $"deleted-{Guid.NewGuid():N}";
        var id = await UploadAndPublishAsync(slug);

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/public-html/{id}");
        Client.DefaultRequestHeaders.Authorization = null;
        var publicResponse = await Client.GetAsync($"/html/{slug}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        publicResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> UploadAndPublishAsync(string slug)
    {
        var response = await UploadAsync(slug);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await ReadCreatedIdAsync(response);
        var publishResponse = await Client.PutAsJsonAsync(
            $"/api/public-html/{id}/publish",
            new { IsPublished = true });
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        return id;
    }

    private async Task<HttpResponseMessage> UploadAsync(string slug)
    {
        using var form = CreateUploadForm(slug, "page.html", "<!doctype html><html><body>published page content</body></html>");
        return await Client.PostAsync("/api/public-html/", form);
    }

    private static MultipartFormDataContent CreateUploadForm(string slug, string fileName, string content)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(slug), "slug");
        form.Add(new StringContent("Integration test page"), "title");
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        file.Headers.ContentType = new MediaTypeHeaderValue("text/html");
        form.Add(file, "file", fileName);
        return form;
    }

    private static async Task<Guid> ReadCreatedIdAsync(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<CreatedPageResponse>(JsonOptions);
        return result!.Id;
    }

    private async Task<string> AuthenticateTokenForAdminAsync()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var email = $"public-html-cleanup-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", Permissions.GetAll());
        return await AuthenticateAsync(email, "Test@Pass123");
    }

    private sealed record CreatedPageResponse(Guid Id);
}
