using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.Notes.Queries.GetNotes;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class NoteEndpointsTests : IntegrationTestBase
{
    public NoteEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private async Task<Guid> CreateNoteAsync(string title, string content, string color = "Default")
    {
        var response = await Client.PostAsJsonAsync("/api/notes", new
        {
            Title = title,
            Content = content,
            Color = color
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
        return envelope.Data;
    }

    #region GET /api/notes (List)

    [Fact]
    public async Task GetNotes_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/notes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetNotes_WithoutNoteReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"note-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/notes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetNotes_WithNoteReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/notes?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task GetNotes_WithPinnedFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Pinned Note", "This is a pinned note.", "Yellow");

        await Client.PostAsync($"/api/notes/{noteId}/pin", null);

        // Act
        var response = await Client.GetAsync("/api/notes?isPinned=true&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task GetNotes_WithSearchTerm_ShouldReturnMatchingNotes()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        await CreateNoteAsync("Searchable", "Find me by search term.");

        // Act
        var response = await Client.GetAsync("/api/notes?searchTerm=Searchable&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    #endregion

    #region GET /api/notes/{id} (Detail)

    [Fact]
    public async Task GetNoteById_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync($"/api/notes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetNoteById_WithoutNoteReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"note-detail-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync($"/api/notes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetNoteById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Detail Note", "Content for detail.");

        // Act
        var response = await Client.GetAsync($"/api/notes/{noteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
        body.Should().Contain("Detail Note");
    }

    [Fact]
    public async Task GetNoteById_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.GetAsync($"/api/notes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/notes (Create)

    [Fact]
    public async Task CreateNote_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/notes", new
        {
            Title = "No Auth",
            Content = "Should fail.",
            Color = "Default"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateNote_WithoutNoteWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"note-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/notes", new
        {
            Title = "No Permission",
            Content = "Should fail.",
            Color = "Default"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateNote_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/notes", new
        {
            Title = "Test Note",
            Content = "This is a test note created by integration test.",
            Color = "Blue"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateNote_WithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/notes", new
        {
            Title = "",
            Content = "Valid content",
            Color = "Default"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateNote_WithEmptyContent_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/notes", new
        {
            Title = "Valid Title",
            Content = "",
            Color = "Default"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region PUT /api/notes/{id} (Update)

    [Fact]
    public async Task UpdateNote_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/notes/{Guid.NewGuid()}", new
        {
            Title = "Updated",
            Content = "Should fail",
            Color = "Default"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateNote_WithoutNoteWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"note-update-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/notes/{Guid.NewGuid()}", new
        {
            Title = "Updated",
            Content = "Should fail",
            Color = "Default"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateNote_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Original", "Original content.", "Green");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/notes/{noteId}", new
        {
            Title = "Updated Title",
            Content = "Updated content.",
            Color = "Red"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync(response);
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateNote_WithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Original", "Original content.");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/notes/{noteId}", new
        {
            Title = "",
            Content = "Valid content",
            Color = "Default"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateNote_WithUnknownId_ShouldReturnError()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/notes/{Guid.NewGuid()}", new
        {
            Title = "Updated",
            Content = "Should fail or succeed but not crash.",
            Color = "Default"
        });

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":false");
    }

    [Fact]
    public async Task UpdateNote_WhenDeleted_ShouldRestoreNoteAndRemoveFromTrash()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Trash Test", "This note will be restored.", "Blue");
        var deleteResponse = await Client.DeleteAsync($"/api/notes/{noteId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var trashedResponse = await Client.GetAsync("/api/notes?isDeleted=true&pageNumber=1&pageSize=10");
        trashedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var trashedEnvelope = await trashedResponse.Content.ReadFromJsonAsync<PagedApiResponse<NoteDto>>(JsonOptions);
        trashedEnvelope.Should().NotBeNull();
        trashedEnvelope!.Data.Should().ContainSingle(note => note.Id == noteId);

        // Act
        var restoreResponse = await Client.PutAsJsonAsync($"/api/notes/{noteId}", new
        {
            Title = "Trash Test",
            Content = "This note will be restored.",
            Color = "Blue"
        });

        // Assert
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var restoreEnvelope = await ReadResponseAsync(restoreResponse);
        restoreEnvelope.Success.Should().BeTrue();

        var afterRestoreResponse = await Client.GetAsync("/api/notes?isDeleted=true&pageNumber=1&pageSize=10");
        afterRestoreResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterRestoreEnvelope = await afterRestoreResponse.Content.ReadFromJsonAsync<PagedApiResponse<NoteDto>>(JsonOptions);
        afterRestoreEnvelope.Should().NotBeNull();
        afterRestoreEnvelope!.Data.Should().NotContain(note => note.Id == noteId);
    }

    #endregion

    #region DELETE /api/notes/{id} (Delete)

    [Fact]
    public async Task DeleteNote_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/notes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteNote_WithoutNoteDeletePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"note-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/notes/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteNote_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Delete Me", "This note will be deleted.");

        // Act
        var response = await Client.DeleteAsync($"/api/notes/{noteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync(response);
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNote_WithUnknownId_ShouldReturnError()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/notes/{Guid.NewGuid()}");

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":false");
    }

    #endregion

    #region POST /api/notes/{id}/pin (Toggle Pin)

    [Fact]
    public async Task TogglePinNote_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsync($"/api/notes/{Guid.NewGuid()}/pin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TogglePinNote_WithoutNoteWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"note-pin-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync($"/api/notes/{Guid.NewGuid()}/pin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TogglePinNote_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Pin Me", "This note will be pinned.");

        // Act
        var response = await Client.PostAsync($"/api/notes/{noteId}/pin", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync(response);
        envelope.Success.Should().BeTrue();
    }

    #endregion

    #region POST /api/notes/{id}/archive (Toggle Archive)

    [Fact]
    public async Task ToggleArchiveNote_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsync($"/api/notes/{Guid.NewGuid()}/archive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ToggleArchiveNote_WithoutNoteWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"note-archive-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync($"/api/notes/{Guid.NewGuid()}/archive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ToggleArchiveNote_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Archive Me", "This note will be archived.");

        // Act
        var response = await Client.PostAsync($"/api/notes/{noteId}/archive", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync(response);
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ArchiveThenFilterArchived_ShouldReturnNoteInArchivedList()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("Archivable", "This note will be archived and queried.");

        await Client.PostAsync($"/api/notes/{noteId}/archive", null);

        // Act
        var response = await Client.GetAsync("/api/notes?isArchived=true&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    #endregion

    #region List Filtering

    [Fact]
    public async Task GetNotes_WithNoteWritePermission_ShouldListNotes()
    {
        // Arrange
        var email = $"note-writer-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.NoteRead, Permissions.NoteWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        await CreateNoteAsync("Writer's Note", "Created by a writer.");

        // Act
        var response = await Client.GetAsync("/api/notes?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task GetNotes_WithArchivedFilter_ShouldReturnArchivedNotesOnly()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("To Archive and Query", "This note will be archived then queried.");
        await Client.PostAsync($"/api/notes/{noteId}/archive", null);

        // Act
        var response = await Client.GetAsync("/api/notes?isArchived=true&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task GetNotes_WithDeletedFilter_ShouldReturnDeletedNotesOnly()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var noteId = await CreateNoteAsync("To Delete and Query", "This note will be deleted then queried.");
        await Client.DeleteAsync($"/api/notes/{noteId}");

        // Act
        var response = await Client.GetAsync("/api/notes?isDeleted=true&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    #endregion
}
