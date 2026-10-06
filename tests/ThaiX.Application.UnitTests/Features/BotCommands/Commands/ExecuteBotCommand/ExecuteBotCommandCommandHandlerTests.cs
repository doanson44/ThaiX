using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;
using ThaiX.Application.Features.BotCommands.Common;
using ThaiX.Application.Features.BotCommands.Services;

namespace ThaiX.Application.UnitTests.Features.BotCommands.Commands.ExecuteBotCommand;

public sealed class ExecuteBotCommandCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUnknownCommand_ShouldReturnRejectedResult()
    {
        // Arrange
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

        var parser = new BotCommandParser();
        var registry = new BotCommandRegistry([]);
        var sut = new ExecuteBotCommandCommandHandler(parser, registry, currentUserMock.Object);

        var command = new ExecuteBotCommandCommand
        {
            RawText = "/unknown",
            Channel = "Slack",
            ExternalChannelId = "C123"
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BotCommandExecutionStatus.Rejected);
        result.PlainText.Should().Contain("Unknown command");
    }

    [Fact]
    public async Task Handle_WithMissingPermission_ShouldReturnRejectedResult()
    {
        // Arrange
        var module = new TestBotCommandModule(new BotCommandMetadata
        {
            Name = "secure",
            Aliases = ["sec"],
            Syntax = "/secure",
            Description = "Secure command",
            Category = BotCommandCategory.System,
            RequiredPermissions = ["Trading.View"],
            SupportsAsync = false
        });

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        currentUserMock.Setup(x => x.HasAllPermissions(It.IsAny<string[]>())).Returns(false);

        var parser = new BotCommandParser();
        var registry = new BotCommandRegistry([module]);
        var sut = new ExecuteBotCommandCommandHandler(parser, registry, currentUserMock.Object);

        var command = new ExecuteBotCommandCommand
        {
            RawText = "/secure",
            Channel = "Slack",
            ExternalChannelId = "C123"
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BotCommandExecutionStatus.Rejected);
        result.ErrorCode.Should().Be(Application.Common.Constants.ErrorCodes.INSUFFICIENT_PERMISSIONS);
    }

    [Fact]
    public async Task Handle_WithAuthorizedCommand_ShouldExecuteModule()
    {
        // Arrange
        var module = new TestBotCommandModule(new BotCommandMetadata
        {
            Name = "ping",
            Aliases = ["p"],
            Syntax = "/ping",
            Description = "Ping command",
            Category = BotCommandCategory.System,
            RequiredPermissions = [],
            SupportsAsync = false
        });

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);

        var parser = new BotCommandParser();
        var registry = new BotCommandRegistry([module]);
        var sut = new ExecuteBotCommandCommandHandler(parser, registry, currentUserMock.Object);

        var command = new ExecuteBotCommandCommand
        {
            RawText = "/ping",
            Channel = "Slack",
            ExternalChannelId = "C123"
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(BotCommandExecutionStatus.Completed);
        result.PlainText.Should().Be("pong");
        module.ExecuteCallCount.Should().Be(1);
    }

    private sealed class TestBotCommandModule : IBotCommandModule
    {
        public TestBotCommandModule(BotCommandMetadata metadata)
        {
            Metadata = metadata;
        }

        public int ExecuteCallCount { get; private set; }

        public BotCommandMetadata Metadata { get; }

        public Task<BotCommandExecutionDto> ExecuteAsync(BotCommandContext context, CancellationToken cancellationToken)
        {
            ExecuteCallCount++;
            return Task.FromResult(new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Completed,
                PlainText = "pong"
            });
        }
    }
}
