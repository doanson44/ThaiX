using System.Net;
using System.Net.Http.Json;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class AiEndpointsTests : IntegrationTestBase
{
    public AiEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GenerateAiText_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/ai/generate", new
        {
            Prompt = "Explain BTC trend"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateAiText_WithoutSystemAdminPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"ai-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/ai/generate", new
        {
            Prompt = "Explain BTC trend"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GenerateAiText_WithBlankPrompt_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"ai-admin-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/ai/generate", new
        {
            Prompt = "   ",
            SystemPrompt = "system"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Code.Should().Be("VAL_INVALID_REQUEST");
    }

    [Fact]
    public async Task GenerateAiText_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var email = $"ai-success-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/ai/generate", new
        {
            Prompt = "Summarize market outlook",
            SystemPrompt = "Be concise"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<GenerateAiTextResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Provider.Should().Be("FakeProvider");
        envelope.Data.Model.Should().Be("fake-model");
        envelope.Data.Text.Should().Be("Generated: Summarize market outlook");
    }

    private sealed record GenerateAiTextResponseDto
    {
        public string Provider { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
    }
}