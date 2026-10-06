using ThaiX.Domain.Aggregates.Outbox;

namespace ThaiX.Application.UnitTests.Domain.Aggregates;

public sealed class OutboxMessageTests
{
    [Fact]
    public void Constructor_WithValidParams_ShouldCreateMessage()
    {
        // Act
        var message = new OutboxMessage("OrderCreated", "{\"orderId\":1}");

        // Assert
        message.Type.Should().Be("OrderCreated");
        message.Content.Should().Be("{\"orderId\":1}");
        message.OccurredOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        message.ProcessedOnUtc.Should().BeNull();
        message.Error.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Constructor_WithInvalidType_ShouldThrow(string? type)
    {
        var act = () => new OutboxMessage(type!, "{\"data\":1}");

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("type");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Constructor_WithInvalidContent_ShouldThrow(string? content)
    {
        var act = () => new OutboxMessage("EventType", content!);

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("content");
    }

    [Fact]
    public void MarkAsProcessed_ShouldSetTimestampAndClearError()
    {
        var message = new OutboxMessage("Type", "Content");

        message.MarkAsProcessed();

        message.ProcessedOnUtc.Should().NotBeNull();
        message.ProcessedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        message.Error.Should().BeNull();
    }

    [Fact]
    public void MarkAsFailed_WithValidError_ShouldSetErrorAndTimestamp()
    {
        var message = new OutboxMessage("Type", "Content");

        message.MarkAsFailed("Connection timeout");

        message.ProcessedOnUtc.Should().NotBeNull();
        message.Error.Should().Be("Connection timeout");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void MarkAsFailed_WithInvalidError_ShouldThrow(string? error)
    {
        var message = new OutboxMessage("Type", "Content");

        var act = () => message.MarkAsFailed(error!);

        act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("error");
    }
}
