using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

public sealed class FakeAiTextGenerationService : IAiTextGenerationService
{
    public Task<AiTextGenerationResult> GenerateAsync(string prompt, string? systemPrompt, CancellationToken ct)
    {
        // Check if this is a lottery triple prediction prompt
        if (prompt.Contains("Power 6/55 Lottery Triple Prediction", StringComparison.Ordinal))
        {
            return Task.FromResult(new AiTextGenerationResult(
                "FakeProvider",
                "fake-model",
                """
                {
                    "numbers": [1, 2, 3],
                    "score": 95,
                    "reasoning": "Fake AI reasoning for triple prediction"
                }
                """));
        }

        // Default behavior for other prompts
        return Task.FromResult(new AiTextGenerationResult(
            "FakeProvider",
            "fake-model",
            $"Generated: {prompt.Trim()}"));
    }
}