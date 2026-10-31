namespace ThaiX.Application.Common.Interfaces;

public sealed record CmsxProxyTestRequest(
    string Url,
    string Method,
    Dictionary<string, string>? Headers,
    Dictionary<string, string>? QueryParameters,
    string? Body,
    int TimeoutSeconds = 30);

public sealed record CmsxProxyTestResponse(
    int ProxyStatusCode,
    string? ProxyReasonPhrase,
    int? TargetStatusCode,
    string? TargetReasonPhrase,
    string Body,
    long DurationMs);

public interface ICmsxProxyDiagnosticService
{
    Task<CmsxProxyTestResponse> SendAsync(
        CmsxProxyTestRequest request,
        CancellationToken cancellationToken);
}
