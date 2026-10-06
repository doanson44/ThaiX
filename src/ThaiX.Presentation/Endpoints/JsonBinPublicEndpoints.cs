using MediatR;
using ThaiX.Application.Features.JsonBins.Queries.GetSharedJsonBin;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Anonymous JsonBin share endpoints. Register alongside MapJsonBinEndpoints.
/// </summary>
public static class JsonBinPublicEndpoints
{
    public static void MapJsonBinPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/public/json-bins")
            .WithTags("JsonBins.Public");

        group.MapGet("/share/{token}", async (
            string token,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSharedJsonBinQuery { Token = token }, ct);
            if (result is null)
                return Results.NotFound();

            var response = ApiResponse<JsonBinSharedDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous();
    }
}
