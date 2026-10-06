using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Common.Interfaces;

public interface IAiTextGenerationService
{
    Task<AiTextGenerationResult> GenerateAsync(
        string prompt,
        string? systemPrompt,
        CancellationToken ct);
}