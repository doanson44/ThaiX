namespace ThaiX.Infrastructure.ExternalApis.Core;

/// <summary>
/// Executes external API requests through a transport.
/// </summary>
public interface IApiTransport
{
    Task<T?> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken);
}
