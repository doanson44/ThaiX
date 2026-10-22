using MediatR;
using ThaiX.Application.Features.BotCommands.Services;
using ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;
using ThaiX.Application.Features.JsonBins.Commands.UpdateJsonBin;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinByCode;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Services;

public sealed class TelegramConversationHistoryServiceTests
{
    [Fact]
    public async Task RecordIncomingMessage_WhenHistoryDoesNotExist_CreatesJsonBin()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(It.IsAny<GetJsonBinByCodeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((JsonBinDetailDto?)null);
        mediator
            .Setup(x => x.Send(It.IsAny<CreateJsonBinCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TELEGRAM_CHAT_-100123");

        var sut = new TelegramConversationHistoryService(mediator.Object);

        // Act
        var result = await sut.RecordIncomingMessageAsync(
            "-100123", 42, "Alice", "hello group", CancellationToken.None);

        // Assert
        result.IsDuplicate.Should().BeFalse();
        result.RecentMessages.Should().BeEmpty();
        mediator.Verify(
            x => x.Send(
                It.Is<CreateJsonBinCommand>(command =>
                    command.Code == "TELEGRAM_CHAT_-100123"
                    && command.ContentJson.Contains("hello group")
                    && command.Category == JsonBinCategories.Temp),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecordIncomingMessage_WhenMessageWasAlreadyStored_SkipsDuplicate()
    {
        // Arrange
        const string contentJson = "[{\"role\":\"user\",\"senderName\":\"Alice\",\"content\":\"hello group\",\"messageId\":42,\"timestampUtc\":\"2026-10-22T00:00:00Z\"}]";
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(It.IsAny<GetJsonBinByCodeQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDetail(contentJson));

        var sut = new TelegramConversationHistoryService(mediator.Object);

        // Act
        var result = await sut.RecordIncomingMessageAsync(
            "-100123", 42, "Alice", "hello group", CancellationToken.None);

        // Assert
        result.IsDuplicate.Should().BeTrue();
        result.RecentMessages.Should().ContainSingle()
            .Which.Content.Should().Be("hello group");
        mediator.Verify(
            x => x.Send(It.IsAny<CreateJsonBinCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
        mediator.Verify(
            x => x.Send(It.IsAny<UpdateJsonBinCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static JsonBinDetailDto CreateDetail(string contentJson) => new()
    {
        Id = Guid.NewGuid(),
        Code = "TELEGRAM_CHAT_-100123",
        Name = "Telegram conversation -100123",
        Category = JsonBinCategories.Temp,
        ContentType = "application/json",
        SizeBytes = contentJson.Length,
        IsCompressed = false,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow,
        ContentJson = contentJson
    };
}
