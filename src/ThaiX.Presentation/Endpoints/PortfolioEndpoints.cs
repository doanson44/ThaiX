using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Portfolios.Commands.CreatePortfolio;
using ThaiX.Application.Features.Portfolios.Commands.DeletePortfolio;
using ThaiX.Application.Features.Portfolios.Commands.ImportPortfolios;
using ThaiX.Application.Features.Portfolios.Commands.UpdatePortfolio;
using ThaiX.Application.Features.Portfolios.Queries.ExportPortfolios;
using ThaiX.Application.Features.Portfolios.Queries.GetPortfolioDetail;
using ThaiX.Application.Features.Portfolios.Queries.GetPortfolios;
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class PortfolioEndpoints
{
    public static void MapPortfolioEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/portfolios")
            .WithTags("Portfolios");

        // GET /api/portfolios
        group.MapGet("/", async (
            [AsParameters] PortfolioQueryParameters parameters,
            IMediator mediator,
            ICurrentUserService currentUser,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPortfoliosQuery
            {
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 20,
                SearchTerm = parameters.SearchTerm,
                OwnerId = currentUser.UserId
            }, ct);
            var response = result.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PortfolioRead);

        // GET /api/portfolios/{id}
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPortfolioDetailQuery(id), ct);
            if (result is null) return Results.NotFound();
            var response = ApiResponse<PortfolioDetailDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PortfolioRead);

        // POST /api/portfolios
        group.MapPost("/", async (CreatePortfolioCommand command, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/portfolios/{id}", response);
        })
        .RequireAuthorization(Permissions.PortfolioWrite);

        // PUT /api/portfolios/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdatePortfolioRequest request, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new UpdatePortfolioCommand
            {
                Id = id,
                Name = request.Name,
                PortfolioType = request.PortfolioType,
                Description = request.Description
            }, ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PortfolioWrite);

        // DELETE /api/portfolios/{id}
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, HttpContext httpContext, CancellationToken ct) =>
        {
            await mediator.Send(new DeletePortfolioCommand(id), ct);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PortfolioDelete);

        // GET /api/portfolios/import/template
        group.MapGet("/import/template", () =>
        {
            const string headerLine = "Name,PortfolioType,Description";
            var bytes = System.Text.Encoding.UTF8.GetBytes(headerLine + "\r\n");
            return Results.File(bytes, "text/csv", "portfolios_import_template.csv");
        })
        .RequireAuthorization(Permissions.PortfolioRead);

        // POST /api/portfolios/import
        group.MapPost("/import", async (
            IFormFile file,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await mediator.Send(new ImportPortfoliosCommand { CsvStream = stream }, ct);
            var response = ApiResponse<ImportResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Permissions.PortfolioWrite)
        .DisableAntiforgery();

        // GET /api/portfolios/export
        group.MapGet("/export", async (
            [AsParameters] ExportPortfolioQueryParameters parameters,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ExportPortfoliosQuery
            {
                SearchTerm = parameters.SearchTerm
            }, ct);

            return Results.File(result.FileContent, "text/csv", result.FileName);
        })
        .RequireAuthorization(Permissions.PortfolioRead);
    }
}

public sealed record UpdatePortfolioRequest(string Name, PortfolioType PortfolioType, string? Description);

public sealed record PortfolioQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(20)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}

public sealed record ExportPortfolioQueryParameters
{
    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}
