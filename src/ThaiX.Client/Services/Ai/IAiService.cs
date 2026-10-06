using ThaiX.Client.Models.Ai;

namespace ThaiX.Client.Services.Ai;

public interface IAiService
{
    Task<GenerateAiTextResponse> GenerateAsync(
        GenerateAiTextRequest request,
        CancellationToken cancellationToken = default);
}