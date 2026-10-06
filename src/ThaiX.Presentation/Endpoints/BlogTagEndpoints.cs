using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Features.Blog.Tags.Commands.CreateTag;
using ThaiX.Application.Features.Blog.Tags.Commands.DeleteTag;
using ThaiX.Application.Features.Blog.Tags.Queries.SearchTags;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class BlogTagEndpoints
{
    public static void MapBlogTagEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/blog/tags")
            .WithTags("Blog Tags");

        // GET /api/blog/tags?search=...&pageNumber=1&pageSize=20
        group.MapGet("/", async (
            [AsParameters] BlogTagQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchTagsQuery
            {
                Search = parameters.Search,
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogRead)
        .WithName("SearchBlogTags")
        .WithDescription("Search/autocomplete blog tags by name.");

        // POST /api/blog/tags
        group.MapPost("/", async (
            [FromBody] CreateTagCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/blog/tags/{id}", response);
        })
        .RequireAuthorization(Permissions.BlogWrite)
        .WithName("CreateBlogTag")
        .WithDescription("Get-or-create a blog tag by name.");

        // DELETE /api/blog/tags/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteTagCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogDelete)
        .WithName("DeleteBlogTag")
        .WithDescription("Soft-delete a blog tag.");
    }
}

public sealed record BlogTagQueryParameters
{
    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    [FromQuery(Name = "pageNumber")]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }
}
