using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.DeveloperTools.CmsxProxyTester.Commands.TestCmsxProxy;

public sealed class TestCmsxProxyCommandHandler(
    ICmsxProxyDiagnosticService diagnosticService)
    : IRequestHandler<TestCmsxProxyCommand, CmsxProxyTestResponse>
{
    public Task<CmsxProxyTestResponse> Handle(
        TestCmsxProxyCommand request,
        CancellationToken cancellationToken)
    {
        return diagnosticService.SendAsync(
            new CmsxProxyTestRequest(
                request.Url,
                request.Method,
                request.Headers,
                request.QueryParameters,
                request.Body,
                request.TimeoutSeconds),
            cancellationToken);
    }
}
