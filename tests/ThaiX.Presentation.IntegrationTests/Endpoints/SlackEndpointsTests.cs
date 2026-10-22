using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class SlackEndpointsTests : IntegrationTestBase
{
    public SlackEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ExecuteCommand_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/slack/commands/execute", new
        {
            RawText = "help",
            ExternalUserId = "slack-user-1",
            ExternalChannelId = "channel-1"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExecuteCommand_WithoutBotCommandExecutePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"slack-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/slack/commands/execute", new
        {
            RawText = "help",
            ExternalUserId = "slack-user-1",
            ExternalChannelId = "channel-1"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExecuteCommand_WithValidRawText_ShouldReturnOk()
    {
        // Arrange
        var email = $"slack-ok-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BotCommandExecute });
        await AuthenticateAsync(email, "Test@Pass123");

        var request = new
        {
            RawText = "help",
            ExternalUserId = "slack-user-2",
            ExternalChannelId = "channel-2"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/slack/commands/execute", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetChannels_WithSystemAdminPermission_ShouldReturnOk()
    {
        // Arrange
        var email = $"slack-admin-channels-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.SystemAdmin });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/slack/commands/channels");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExecutionStatus_WithUnknownExecutionId_ShouldReturnNotFound()
    {
        // Arrange
        var email = $"slack-admin-status-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BotCommandExecute });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync($"/api/slack/commands/executions/{Guid.NewGuid():N}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Execute_ConversationMentionWithPriorMessage_ShouldPassContextAndReply()
    {
        // Arrange
        var email = $"slack-context-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BotCommandExecute });
        await AuthenticateAsync(email, "Test@Pass123");

        var channelId = $"channel-{Guid.NewGuid():N}";
        var threadTs = "1710000000.000001";
        var slackNotifications = Factory.Services.GetRequiredService<FakeSlackNotificationService>();
        var responder = Factory.Services.GetRequiredService<FakeBotConversationalResponder>();
        slackNotifications.Reset();
        responder.Reset();

        // Act
        var incomingResponse = await Client.PostAsJsonAsync("/api/slack/commands/execute", new
        {
            Text = "The deployment is failing",
            ExternalUserId = "slack-context-user",
            ExternalChannelId = channelId,
            IsConversationEvent = true,
            IsMention = false,
            MessageTs = "1710000000.000002",
            ConversationThreadTs = threadTs,
            SenderName = "Alice"
        });

        var mentionResponse = await Client.PostAsJsonAsync("/api/slack/commands/execute", new
        {
            Text = "Can you summarize the issue?",
            ExternalUserId = "slack-context-user",
            ExternalChannelId = channelId,
            ThreadTs = threadTs,
            IsConversationEvent = true,
            IsMention = true,
            MessageTs = "1710000000.000003",
            ConversationThreadTs = threadTs,
            SenderName = "Bob"
        });

        // Assert
        incomingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        mentionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        responder.CallCount.Should().Be(1);
        responder.LastMessage.Should().Contain("The deployment is failing");
        responder.LastMessage.Should().Contain("Can you summarize the issue?");
        slackNotifications.ChannelMessages.Should().Contain("Integration test response");
    }

}
