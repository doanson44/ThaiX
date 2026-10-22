using MediatR;
using ThaiX.Application.Features.BotCommands.Services;
using ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;
using ThaiX.Application.Features.JsonBins.Commands.UpdateJsonBin;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinByCode;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Services;

public sealed class SlackConversationHistoryServiceTests
{
    [Fact]
    public async Task RecordIncomingMessage_WhenHistoryDoesNotExist_CreatesChannelJsonBin()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetJsonBinByCodeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((JsonBinDetailDto?)null);
        mediator.Setup(x => x.Send(It.IsAny<CreateJsonBinCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("SLACK_CHANNEL_C123");
        var sut = new SlackConversationHistoryService(mediator.Object);

        // Act
        var result = await sut.RecordIncomingMessageAsync(
            "C123", null, "1720000000.000001", "Alice", "hello channel", CancellationToken.None);

        // Assert
        result.IsDuplicate.Should().BeFalse();
        result.RecentMessages.Should().BeEmpty();
        mediator.Verify(x => x.Send(
            It.Is<CreateJsonBinCommand>(command => command.Code == "SLACK_CHANNEL_C123"
                && command.ContentJson.Contains("hello channel") && command.Category == JsonBinCategories.Temp),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordIncomingMessage_WhenMessageWasAlreadyStored_SkipsDuplicate()
    {
        // Arrange
        const string contentJson = "[{\"role\":\"user\",\"senderName\":\"Alice\",\"content\":\"hello thread\",\"messageId\":\"1720000000.000001\",\"timestampUtc\":\"2026-10-22T00:00:00Z\"}]";
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<GetJsonBinByCodeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDetail(contentJson));
        var sut = new SlackConversationHistoryService(mediator.Object);

        // Act
        var result = await sut.RecordIncomingMessageAsync(
            "C123", "1720000000.000000", "1720000000.000001", "Alice", "hello thread", CancellationToken.None);

        // Assert
        result.IsDuplicate.Should().BeTrue();
        result.RecentMessages.Should().ContainSingle().Which.Content.Should().Be("hello thread");
        mediator.Verify(x => x.Send(It.IsAny<CreateJsonBinCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        mediator.Verify(x => x.Send(It.IsAny<UpdateJsonBinCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static JsonBinDetailDto CreateDetail(string contentJson) => new()
    {
        Id = Guid.NewGuid(), Code = "SLACK_THREAD_C123_1720000000.000000", Name = "Slack conversation thread",
        Category = JsonBinCategories.Temp, ContentType = "application/json", SizeBytes = contentJson.Length,
        IsCompressed = false, CreatedAtUtc = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, ContentJson = contentJson
    };
}
