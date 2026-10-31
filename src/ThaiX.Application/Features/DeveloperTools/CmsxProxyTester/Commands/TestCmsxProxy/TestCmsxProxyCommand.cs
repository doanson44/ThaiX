using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.DeveloperTools.CmsxProxyTester.Commands.TestCmsxProxy;

public sealed record TestCmsxProxyCommand(
    string Url,
    string Method,
    Dictionary<string, string>? Headers,
    Dictionary<string, string>? QueryParameters,
    string? Body,
    int TimeoutSeconds = 30) : IRequest<CmsxProxyTestResponse>;
