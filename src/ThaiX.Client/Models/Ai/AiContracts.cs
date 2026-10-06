namespace ThaiX.Client.Models.Ai;

public sealed class GenerateAiTextRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? SystemPrompt { get; set; }
}

public sealed class GenerateAiTextResponse
{
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}