using MediatR;
using Microsoft.AspNetCore.Mvc;
using ThaiX.Application.Features.Blog.Categories;
using ThaiX.Application.Features.Blog.Categories.Commands.CreateCategory;
using ThaiX.Application.Features.Blog.Categories.Commands.DeleteCategory;
using ThaiX.Application.Features.Blog.Categories.Commands.UpdateCategory;
using ThaiX.Application.Features.Blog.Categories.Queries.GetCategories;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class BlogCategoryEndpoints
{
    public static void MapBlogCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/blog/categories")
            .WithTags("Blog Categories");

        // GET /api/blog/categories
        group.MapGet("/", async (
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetCategoriesQuery(), cancellationToken);
            var response = ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("GetBlogCategories")
        .WithDescription("Get the flat list of blog categories.");

        // POST /api/blog/categories
        group.MapPost("/", async (
            [FromBody] CreateCategoryCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/blog/categories/{id}", response);
        })
        .RequireAuthorization(Permissions.BlogWrite)
        .WithName("CreateBlogCategory")
        .WithDescription("Create a new blog category.");

        // PUT /api/blog/categories/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateCategoryRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCategoryCommand
            {
                Id = id,
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                Icon = request.Icon,
                Color = request.Color
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogWrite)
        .WithName("UpdateBlogCategory")
        .WithDescription("Update an existing blog category.");

        // DELETE /api/blog/categories/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.BlogDelete)
        .WithName("DeleteBlogCategory")
        .WithDescription("Soft-delete a blog category.");
    }
}

public sealed record UpdateCategoryRequest
{
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}
