using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Extensions;

/// <summary>
/// Extension methods for <see cref="ApiException"/> to build user-facing error detail
/// including validation errors when present.
/// </summary>
public static class ApiExceptionExtensions
{
    /// <summary>
    /// Returns the exception message; when validation errors exist, appends each one on a new line.
    /// </summary>
    public static string GetDisplayDetail(this ApiException ex)
    {
        if (ex.ValidationErrors.Count == 0)
            return ex.Message;
        return ex.Message + "\n" + string.Join("\n", ex.ValidationErrors);
    }
}
