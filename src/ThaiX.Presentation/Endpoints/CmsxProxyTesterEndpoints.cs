using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.DeveloperTools.CmsxProxyTester.Commands.TestCmsxProxy;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class CmsxProxyTesterEndpoints
{
    public static void MapCmsxProxyTesterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/developer-tools/cmsx-proxy")
            .WithTags("Developer Tools - CMSX Proxy")
            .RequireAuthorization(Permissions.SystemAdmin);

        group.MapPost("/test", async (
            TestCmsxProxyCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<CmsxProxyTestResponse>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .WithName("TestCmsxProxy")
        .WithDescription("Sends a diagnostic request through the configured CMSX proxy. Restricted to System.Admin.");
    }
}
