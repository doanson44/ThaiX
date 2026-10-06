using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class FileEndpointsTests : IntegrationTestBase
{
    public FileEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UploadFile_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        using var content = CreateMultipartContent("hello.txt", "text/plain", "hello", "contacts/test/avatar.txt");

        // Act
        var response = await Client.PostAsync("/api/files/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadFile_WithoutFileWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"file-write-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");
        using var content = CreateMultipartContent("hello.txt", "text/plain", "hello", "contacts/test/avatar.txt");

        // Act
        var response = await Client.PostAsync("/api/files/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadFile_WithMissingStorageKey_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"file-invalid-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.FileWrite });
        await AuthenticateAsync(email, "Test@Pass123");
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("hello"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "hello.txt");

        // Act
        var response = await Client.PostAsync("/api/files/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetFile_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetFile_WithoutFileReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"file-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetFile_WithFileReadPermission_ForUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var email = $"file-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.FileRead });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadAndGetFile_WithValidPermissions_ShouldReturnOk()
    {
        // Arrange
        var email = $"file-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.FileWrite, Permissions.FileRead });
        await AuthenticateAsync(email, "Test@Pass123");
        var fileId = await UploadFileAsync("avatar.txt", "text/plain", "hello world", $"contacts/{Guid.NewGuid():N}/avatar/avatar.txt");

        // Act
        var response = await Client.GetAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var file = await response.Content.ReadFromJsonAsync<FileDto>(JsonOptions);
        file.Should().NotBeNull();
        file!.Id.Should().Be(fileId);
        file.FileName.Should().Be("avatar.txt");
        file.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task DeleteFile_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteFile_WithoutDeletePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"file-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteFile_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var email = $"file-delete-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.FileWrite, Permissions.FileRead, Permissions.FileDelete });
        await AuthenticateAsync(email, "Test@Pass123");
        var fileId = await UploadFileAsync("delete.txt", "text/plain", "delete me", $"contacts/{Guid.NewGuid():N}/docs/delete.txt");

        // Act
        var response = await Client.DeleteAsync($"/api/files/{fileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/files/{fileId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteFile_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var email = $"file-delete-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.FileDelete });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Code.Should().Be("FILE_NOT_FOUND");
    }

    private async Task<Guid> UploadFileAsync(string fileName, string contentType, string content, string storageKey)
    {
        using var multipart = CreateMultipartContent(fileName, contentType, content, storageKey);
        var response = await Client.PostAsync("/api/files/upload", multipart);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var file = await response.Content.ReadFromJsonAsync<FileDto>(JsonOptions);
        file.Should().NotBeNull();
        return file!.Id;
    }

    private static MultipartFormDataContent CreateMultipartContent(string fileName, string contentType, string content, string storageKey)
    {
        var multipart = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        multipart.Add(fileContent, "file", fileName);
        multipart.Add(new StringContent(storageKey), "storageKey");
        return multipart;
    }
}
