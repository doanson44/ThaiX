using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Features.MarketScanner.Commands.CreateMarketScannerRule;
using ThaiX.Application.Features.MarketScanner.Commands.DeleteMarketScannerRule;
using ThaiX.Application.Features.MarketScanner.Commands.UpdateMarketScannerRule;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Endpoints for managing market scanner rules.
/// </summary>
public static class MarketScannerEndpoints
{
    public static void MapMarketScannerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/market-scanner/rules")
            .WithTags("Market Scanner Rules");

        // GET /api/market-scanner/rules
        group.MapGet("/", async (
            [AsParameters] MarketScannerQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMarketScannerRulesQuery
            {
                PageNumber = parameters.PageNumber ?? MarketScannerQueryParameters.DefaultPageNumber,
                PageSize = parameters.PageSize ?? MarketScannerQueryParameters.DefaultPageSize,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
                    ? MarketScannerQueryParameters.DefaultSortBy
                    : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? MarketScannerQueryParameters.DefaultSortDescending,
                SearchTerm = parameters.SearchTerm
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MarketScannerRuleRead)
        .WithName("GetMarketScannerRules")
        .WithDescription("Get paginated list of market scanner rules");

        // POST /api/market-scanner/rules
        group.MapPost("/", async (
            [FromBody] CreateMarketScannerRuleCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/market-scanner/rules/{id}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MarketScannerRuleWrite)
        .WithName("CreateMarketScannerRule")
        .WithDescription("Create a new market scanner rule");

        // PUT /api/market-scanner/rules/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateMarketScannerRuleRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateMarketScannerRuleCommand
            {
                Id = id,
                Name = request.Name,
                Window = request.Window,
                Threshold = request.Threshold,
                IsEnabled = request.IsEnabled
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MarketScannerRuleWrite)
        .WithName("UpdateMarketScannerRule")
        .WithDescription("Update an existing market scanner rule");

        // DELETE /api/market-scanner/rules/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteMarketScannerRuleCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MarketScannerRuleDelete)
        .WithName("DeleteMarketScannerRule")
        .WithDescription("Soft-delete a market scanner rule");
    }
}

/// <summary>
/// Query parameters for the market scanner rules list endpoint.
/// </summary>
public sealed record MarketScannerQueryParameters
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const string DefaultSortBy = "name";
    public const bool DefaultSortDescending = false;

    [FromQuery(Name = "pageNumber")]
    [DefaultValue(DefaultPageNumber)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(DefaultPageSize)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "sortBy")]
    [DefaultValue(DefaultSortBy)]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    [DefaultValue(DefaultSortDescending)]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }
}

/// <summary>
/// Request body for updating a market scanner rule.
/// </summary>
public sealed record UpdateMarketScannerRuleRequest
{
    public required string Name { get; init; }
    public required MarketTimeWindow Window { get; init; }
    public decimal? Threshold { get; init; }
    public required bool IsEnabled { get; init; }
}
