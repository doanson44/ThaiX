using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Features.Blog.Posts;
using ThaiX.Application.Features.Blog.Posts.Commands.ArchivePost;
using ThaiX.Application.Features.Blog.Posts.Commands.CreatePost;
using ThaiX.Application.Features.Blog.Posts.Commands.DeletePost;
using ThaiX.Application.Features.Blog.Posts.Commands.DuplicatePost;
using ThaiX.Application.Features.Blog.Posts.Commands.PublishPost;
using ThaiX.Application.Features.Blog.Posts.Commands.RestorePost;
using ThaiX.Application.Features.Blog.Posts.Commands.SchedulePost;
using ThaiX.Application.Features.Blog.Posts.Commands.UnpublishPost;
using ThaiX.Application.Features.Blog.Posts.Commands.UpdatePost;
using ThaiX.Application.Features.Blog.Posts.Queries.GetPostById;
using ThaiX.Application.Features.Blog.Posts.Queries.GetPostBySlug;
using ThaiX.Application.Features.Blog.Posts.Queries.GetPosts;
using ThaiX.Application.Features.Blog.Posts.Queries.GetPublishedPosts;
using ThaiX.Domain.Aggregates.Blog;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class BlogPostEndpoints
{
    public static void MapBlogPostEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/blog/posts")
            .WithTags("Blog Posts");

        // GET /api/blog/posts (admin, any status)
        group.MapGet("/", async (
            [AsParameters] BlogPostQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPostsQuery
            {
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20,
                SortBy = parameters.SortBy,
                SortDescending = parameters.SortDescending ?? false,
                Status = parameters.Status,
                CategoryId = parameters.CategoryId,
                SearchTerm = parameters.SearchTerm
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogRead)
        .WithName("GetPosts")
        .WithDescription("Get paginated admin post list. Optional filters: status, categoryId, searchTerm.");

        // GET /api/blog/posts/public (anonymous, published only)
        group.MapGet("/public", async (
            [AsParameters] BlogPublishedPostQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPublishedPostsQuery
            {
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20,
                CategoryId = parameters.CategoryId,
                TagId = parameters.TagId
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetPublishedPosts")
        .WithDescription("Get paginated public post list (published only). Optional filters: categoryId, tagId.");

        // GET /api/blog/posts/public/{slug}
        group.MapGet("/public/{slug}", async (
            string slug,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPostBySlugQuery { Slug = slug }, cancellationToken);
            if (result is null)
            {
                return Results.NotFound();
            }

            var response = ApiResponse<PostDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetPublishedPostBySlug")
        .WithDescription("Get a published post by slug for the public post-view page.");

        // GET /api/blog/posts/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetPostByIdQuery { Id = id }, cancellationToken);
            if (result is null)
            {
                return Results.NotFound();
            }

            var response = ApiResponse<PostDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogRead)
        .WithName("GetPostById")
        .WithDescription("Get a post by Id (admin, any status).");

        // POST /api/blog/posts
        group.MapPost("/", async (
            [FromBody] CreatePostCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/blog/posts/{id}", response);
        })
        .RequireAuthorization(Permissions.BlogWrite)
        .WithName("CreatePost")
        .WithDescription("Create a new draft post.");

        // PUT /api/blog/posts/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePostRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePostCommand
            {
                Id = id,
                Title = request.Title,
                Slug = request.Slug,
                Summary = request.Summary,
                ContentHtml = request.ContentHtml,
                FeaturedImageUrl = request.FeaturedImageUrl,
                CategoryId = request.CategoryId,
                MetaTitle = request.MetaTitle,
                MetaDescription = request.MetaDescription,
                TagIds = request.TagIds
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogWrite)
        .WithName("UpdatePost")
        .WithDescription("Update an existing post's content/metadata.");

        // DELETE /api/blog/posts/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeletePostCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogDelete)
        .WithName("DeletePost")
        .WithDescription("Soft-delete a post.");

        // POST /api/blog/posts/{id}/restore
        group.MapPost("/{id:guid}/restore", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RestorePostCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogDelete)
        .WithName("RestorePost")
        .WithDescription("Restore a soft-deleted post.");

        // POST /api/blog/posts/{id}/duplicate
        group.MapPost("/{id:guid}/duplicate", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var newId = await mediator.Send(new DuplicatePostCommand { Id = id }, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(newId);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogWrite)
        .WithName("DuplicatePost")
        .WithDescription("Duplicate a post into a new draft copy.");

        // POST /api/blog/posts/{id}/publish
        group.MapPost("/{id:guid}/publish", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new PublishPostCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogPublish)
        .WithName("PublishPost")
        .WithDescription("Publish a post immediately.");

        // POST /api/blog/posts/{id}/schedule
        group.MapPost("/{id:guid}/schedule", async (
            Guid id,
            [FromBody] SchedulePostRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new SchedulePostCommand { Id = id, PublishAt = request.PublishAt }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogPublish)
        .WithName("SchedulePost")
        .WithDescription("Schedule a post to publish automatically at a future time.");

        // POST /api/blog/posts/{id}/unpublish
        group.MapPost("/{id:guid}/unpublish", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new UnpublishPostCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogPublish)
        .WithName("UnpublishPost")
        .WithDescription("Revert a published/scheduled post back to Draft.");

        // POST /api/blog/posts/{id}/archive
        group.MapPost("/{id:guid}/archive", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new ArchivePostCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogPublish)
        .WithName("ArchivePost")
        .WithDescription("Archive a post.");
    }
}

public sealed record BlogPostQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "status")]
    public PostStatus? Status { get; init; }

    [FromQuery(Name = "categoryId")]
    public Guid? CategoryId { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}

public sealed record BlogPublishedPostQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }

    [FromQuery(Name = "categoryId")]
    public Guid? CategoryId { get; init; }

    [FromQuery(Name = "tagId")]
    public Guid? TagId { get; init; }
}

public sealed record UpdatePostRequest
{
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required string Summary { get; init; }
    public required string ContentHtml { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public Guid? CategoryId { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public IReadOnlyList<Guid> TagIds { get; init; } = [];
}

public sealed record SchedulePostRequest
{
    public required DateTime PublishAt { get; init; }
}
