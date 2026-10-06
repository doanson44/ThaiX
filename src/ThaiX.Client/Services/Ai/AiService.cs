using System.Net.Http.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.Ai;

public sealed class AiService : IAiService
{
    private readonly HttpClient _httpClient;

    public AiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GenerateAiTextResponse> GenerateAsync(
        GenerateAiTextRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/ai/generate",
            request,
            cancellationToken);

        return await ApiResponseReader.ReadSuccessDataAsync<GenerateAiTextResponse>(response, cancellationToken);
    }
}