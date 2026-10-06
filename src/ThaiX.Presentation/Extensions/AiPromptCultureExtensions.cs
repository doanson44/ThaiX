using System.Globalization;

namespace ThaiX.Presentation.Extensions;

/// <summary>
/// Appends an explicit response-language directive to an AI system prompt based on the current
/// request's UI culture (already resolved per-request by app.UseRequestLocalization() from the
/// Accept-Language header the Blazor client sends on every call — see ApiAuthorizationMessageHandler
/// on the client side). Explicit is safer than relying on the model to infer language from the
/// prompt content alone.
/// </summary>
public static class AiPromptCultureExtensions
{
    public static string WithCurrentCultureInstruction(this string systemPrompt)
    {
        var isVietnamese = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals("vi", StringComparison.OrdinalIgnoreCase);

        var languageInstruction = isVietnamese
            ? "Respond in Vietnamese (Tiếng Việt), regardless of what language the input data is in."
            : "Respond in English, regardless of what language the input data is in.";

        return $"{systemPrompt}\n\n{languageInstruction}";
    }
}
