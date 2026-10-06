namespace ThaiX.Application.Common.Models;

public sealed record AiTextGenerationResult(
    string Provider,
    string Model,
    string Text);