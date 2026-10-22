using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

public sealed class FakeBotCommandIntentResolver : IBotCommandIntentResolver
{
    public Task<string?> ResolveCommandTextAsync(string mentionText, CancellationToken cancellationToken)
    {
        var resolvedText = string.Equals(mentionText.Trim(), "help", StringComparison.OrdinalIgnoreCase)
            ? "help"
            : null;
        return Task.FromResult<string?>(resolvedText);
    }
}

public sealed class FakeBotConversationalResponder : IBotConversationalResponder
{
    public string? LastMessage { get; private set; }
    public string? LastSystemPrompt { get; private set; }
    public int CallCount { get; private set; }

    public Task<string?> RespondAsync(string message, string systemPrompt, CancellationToken cancellationToken)
    {
        LastMessage = message;
        LastSystemPrompt = systemPrompt;
        CallCount++;
        return Task.FromResult<string?>("Integration test response");
    }

    public void Reset()
    {
        LastMessage = null;
        LastSystemPrompt = null;
        CallCount = 0;
    }
}
