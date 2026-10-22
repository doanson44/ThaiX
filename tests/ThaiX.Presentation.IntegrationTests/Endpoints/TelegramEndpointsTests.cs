using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class TelegramEndpointsTests : IntegrationTestBase
{
    public TelegramEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Webhook_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/telegram/webhook",
            new { Text = "status", ExternalUserId = "12345", ExternalChatId = "-100123" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Webhook_WithoutBotCommandExecutePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"tele-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/telegram/webhook",
            new { Text = "status", ExternalUserId = "12345", ExternalChatId = "-100123" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Webhook_WithPermission_ValidPayload_ShouldReturnOk()
    {
        // Arrange
        var email = $"tele-ok-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123",
            new[] { Permissions.BotCommandExecute });
        await AuthenticateAsync(email, "Test@Pass123");

        var request = new
        {
            Text = "help",
            ExternalUserId = "123456789",
            ExternalChatId = "-1009876543210"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/telegram/webhook", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Webhook_GroupMentionWithPriorMessage_ShouldPassContextAndReply()
    {
        // Arrange
        var email = $"telegram-context-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BotCommandExecute });
        await AuthenticateAsync(email, "Test@Pass123");

        var chatId = $"-100{Guid.NewGuid():N}";
        var telegramNotifications = Factory.Services.GetRequiredService<FakeTelegramNotificationService>();
        var responder = Factory.Services.GetRequiredService<FakeBotConversationalResponder>();
        telegramNotifications.Reset();
        responder.Reset();

        // Act
        var incomingResponse = await Client.PostAsJsonAsync("/api/telegram/webhook", new
        {
            Text = "The deployment is failing",
            ExternalUserId = "telegram-context-user",
            ExternalChatId = chatId,
            ChatType = "group",
            MessageId = 1001L,
            SenderName = "Alice"
        });

        var mentionResponse = await Client.PostAsJsonAsync("/api/telegram/webhook", new
        {
            Text = "Can you summarize the issue?",
            ExternalUserId = "telegram-context-user",
            ExternalChatId = chatId,
            ChatType = "group",
            MessageId = 1002L,
            SenderName = "Bob",
            IsBotMentioned = true
        });

        // Assert
        incomingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        mentionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responder.CallCount.Should().Be(1);
        responder.LastMessage.Should().Contain("The deployment is failing");
        responder.LastMessage.Should().Contain("Can you summarize the issue?");
        telegramNotifications.ChatMessages.Should().Contain("Integration test response");
    }

}
