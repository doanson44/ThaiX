using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.BotCommands.Services;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Services;

public sealed class BotCommandResponseNarratorTests
{
    [Fact]
    public async Task NarrateAsync_WhenCommandResultHasStructuredMetrics_ShouldSendUserRequestAndMetricsToAi()
    {
        // Arrange
        var aiService = new Mock<IAiTextGenerationService>();
        aiService
            .Setup(x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiTextGenerationResult("test", "test", "The reported change is positive, but the data alone does not establish a trend."));

        var narrator = CreateNarrator(aiService.Object);
        var result = CreateCommandResult();

        // Act
        var narrated = await narrator.NarrateAsync(
            result,
            new BotCommandNarrationOptions
            {
                Enabled = true,
                UserMessage = "Analyze BTC and explain the main risks.",
                SystemPrompt = "Use evidence only."
            },
            CancellationToken.None);

        // Assert
        narrated.PlainText.Should().Contain("does not establish a trend");
        aiService.Verify(x => x.GenerateAsync(
            It.Is<string>(prompt =>
                prompt.Contains("Analyze BTC and explain the main risks.", StringComparison.Ordinal)
                && prompt.Contains("Price: 67000", StringComparison.Ordinal)
                && prompt.Contains("Change: 2.5%", StringComparison.Ordinal)
                && prompt.Contains("Never reproduce the raw serialization", StringComparison.Ordinal)),
            It.Is<string?>(systemPrompt => systemPrompt == "Use evidence only."),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NarrateAsync_WhenAiGenerationFails_ShouldClearRawTextAndPreserveStructuredCard()
    {
        // Arrange
        var aiService = new Mock<IAiTextGenerationService>();
        aiService
            .Setup(x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider unavailable"));

        var narrator = CreateNarrator(aiService.Object);
        var result = CreateCommandResult();

        // Act
        var narrated = await narrator.NarrateAsync(
            result,
            new BotCommandNarrationOptions { Enabled = true },
            CancellationToken.None);

        // Assert
        narrated.PlainText.Should().BeEmpty();
        narrated.Card.Should().BeSameAs(result.Card);
    }

    [Fact]
    public async Task NarrateAsync_WhenAiReturnsEmptyText_ShouldClearRawTextAndPreserveStructuredCard()
    {
        // Arrange
        var aiService = new Mock<IAiTextGenerationService>();
        aiService
            .Setup(x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiTextGenerationResult("test", "test", string.Empty));

        var narrator = CreateNarrator(aiService.Object);
        var result = CreateCommandResult();

        // Act
        var narrated = await narrator.NarrateAsync(
            result,
            new BotCommandNarrationOptions { Enabled = true },
            CancellationToken.None);

        // Assert
        narrated.PlainText.Should().BeEmpty();
        narrated.Card.Should().BeSameAs(result.Card);
    }

    [Fact]
    public async Task NarrateAsync_WhenAiGenerationFailsWithoutCard_ShouldPreservePlainTextConfirmation()
    {
        // Arrange
        var aiService = new Mock<IAiTextGenerationService>();
        aiService
            .Setup(x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Provider unavailable"));

        var narrator = CreateNarrator(aiService.Object);
        var result = CreateTextOnlyCommandResult();

        // Act
        var narrated = await narrator.NarrateAsync(
            result,
            new BotCommandNarrationOptions { Enabled = true },
            CancellationToken.None);

        // Assert
        narrated.PlainText.Should().Be(result.PlainText);
        narrated.Card.Should().BeNull();
    }

    [Fact]
    public async Task NarrateAsync_WhenAiReturnsEmptyTextWithoutCard_ShouldPreservePlainTextConfirmation()
    {
        // Arrange
        var aiService = new Mock<IAiTextGenerationService>();
        aiService
            .Setup(x => x.GenerateAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiTextGenerationResult("test", "test", string.Empty));

        var narrator = CreateNarrator(aiService.Object);
        var result = CreateTextOnlyCommandResult();

        // Act
        var narrated = await narrator.NarrateAsync(
            result,
            new BotCommandNarrationOptions { Enabled = true },
            CancellationToken.None);

        // Assert
        narrated.PlainText.Should().Be(result.PlainText);
        narrated.Card.Should().BeNull();
    }

    private static BotCommandResponseNarrator CreateNarrator(IAiTextGenerationService aiService)
        => new(aiService, Mock.Of<ILogger<BotCommandResponseNarrator>>());

    private static BotCommandExecutionDto CreateCommandResult()
        => new()
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = "BTCUSDT [CryptoSpot]: last=67000, change24h=2.5%.",
            Card = new NotificationCard
            {
                Category = "Crypto Spot",
                Title = "BTCUSDT",
                Type = NotificationCardType.Metric,
                Metrics =
                [
                    new() { Label = "Price", Value = "67000" },
                    new() { Label = "Change", Value = "2.5%" }
                ]
            }
        };

    private static BotCommandExecutionDto CreateTextOnlyCommandResult()
        => new()
        {
            Status = BotCommandExecutionStatus.Completed,
            PlainText = "Alert created successfully. ID: 42."
        };
}
