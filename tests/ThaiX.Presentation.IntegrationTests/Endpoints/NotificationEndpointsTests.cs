using System.Net;
using System.Net.Http.Json;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class NotificationEndpointsTests : IntegrationTestBase
{
    public NotificationEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Send_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", new { EventType = "SystemAlert", Text = "hello" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Send_WithoutSystemAdminPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"notif-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", new { EventType = "SystemAlert", Text = "hello" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Send_WithSystemAdminPermission_InvalidPayload_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"notif-admin-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Send_WithSystemAdminPermission_ValidPayload_ShouldReturnOk()
    {
        // Arrange
        var email = $"notif-ok-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        var request = new
        {
            EventType = "SystemAlert",
            Text = "Test notification from integration test",
            Title = "Test Title",
            Severity = "info",
            Target = "Both"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Send_WithTargetSlack_ShouldReturnOk()
    {
        // Arrange
        var email = $"notif-slack-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        var request = new
        {
            EventType = "PriceUpdate",
            Text = "Price update test",
            Target = "Slack"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Send_WithTargetTelegram_ShouldReturnOk()
    {
        // Arrange
        var email = $"notif-tele-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        var request = new
        {
            EventType = "SystemAlert",
            Text = "Telegram test",
            Target = "Telegram"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Send_WithInvalidTarget_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"notif-invt-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        var request = new
        {
            EventType = "SystemAlert",
            Text = "Invalid target test",
            Target = "WhatsApp"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/notifications/send", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
