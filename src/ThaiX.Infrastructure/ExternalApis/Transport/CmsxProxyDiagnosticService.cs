using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.ExternalApis.Transport;

public sealed class CmsxProxyDiagnosticService(ProxyTransport proxyTransport)
    : ICmsxProxyDiagnosticService
{
    public Task<CmsxProxyTestResponse> SendAsync(
        CmsxProxyTestRequest request,
        CancellationToken cancellationToken)
    {
        return proxyTransport.SendDiagnosticAsync(request, cancellationToken);
    }
}
