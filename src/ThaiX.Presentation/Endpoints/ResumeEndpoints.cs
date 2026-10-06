using MediatR;
using ThaiX.Application.Features.Resumes;
using ThaiX.Application.Features.Resumes.Commands.UpsertResume;
using ThaiX.Application.Features.Resumes.Queries.GetMyResume;
using ThaiX.Application.Features.Resumes.Queries.GetResumeBySlug;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class ResumeEndpoints
{
    public static void MapResumeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/resumes")
            .WithTags("Resumes");

        group.MapGet("/mine", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetMyResumeQuery(), ct);
            if (result is null)
            {
                return Results.NotFound();
            }

            var response = ApiResponse<ResumeDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ResumeRead);

        group.MapPut("/mine", async (
            UpsertResumeCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.ResumeWrite);

        group.MapGet("/public/{slug}", async (
            string slug,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetResumeBySlugQuery { Slug = slug }, ct);
            if (result is null)
            {
                return Results.NotFound();
            }

            var response = ApiResponse<ResumeDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous();
    }
}
