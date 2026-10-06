using System.Net;

namespace ThaiX.Client.Services.Api;

/// <summary>
/// Represents a structured API error returned by the backend envelope.
/// </summary>
public sealed class ApiException : Exception
{
    public ApiException(
        string code,
        string message,
        HttpStatusCode statusCode,
        IReadOnlyList<string>? validationErrors = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        ValidationErrors = validationErrors ?? Array.Empty<string>();
    }

    public string Code { get; }
    public HttpStatusCode StatusCode { get; }
    public IReadOnlyList<string> ValidationErrors { get; }
}
