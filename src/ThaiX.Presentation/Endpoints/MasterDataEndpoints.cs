using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.LookupSymbol;
using ThaiX.Application.Features.MasterData.Banks.Commands.CreateBank;
using ThaiX.Application.Features.MasterData.Banks.Commands.DeleteBank;
using ThaiX.Application.Features.MasterData.Banks.Commands.ImportBanks;
using ThaiX.Application.Features.MasterData.Banks.Commands.UpdateBank;
using ThaiX.Application.Features.MasterData.Banks.Queries.GetBanks;
using ThaiX.Application.Features.MasterData.Cities.Commands.CreateCity;
using ThaiX.Application.Features.MasterData.Cities.Commands.DeleteCity;
using ThaiX.Application.Features.MasterData.Cities.Commands.ImportCities;
using ThaiX.Application.Features.MasterData.Cities.Commands.UpdateCity;
using ThaiX.Application.Features.MasterData.Cities.Queries.GetCities;
using ThaiX.Application.Features.MasterData.Countries.Commands.CreateCountry;
using ThaiX.Application.Features.MasterData.Countries.Commands.DeleteCountry;
using ThaiX.Application.Features.MasterData.Countries.Commands.ImportCountries;
using ThaiX.Application.Features.MasterData.Countries.Commands.UpdateCountry;
using ThaiX.Application.Features.MasterData.Countries.Queries.GetCountries;
using ThaiX.Application.Features.MasterData.Districts.Commands.CreateDistrict;
using ThaiX.Application.Features.MasterData.Districts.Commands.DeleteDistrict;
using ThaiX.Application.Features.MasterData.Districts.Commands.ImportDistricts;
using ThaiX.Application.Features.MasterData.Districts.Commands.UpdateDistrict;
using ThaiX.Application.Features.MasterData.Districts.Queries.GetDistricts;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Master data management endpoints (Countries, Cities, Districts, Banks).
/// All endpoints require appropriate permissions.
/// </summary>
public static class MasterDataEndpoints
{
    public static void MapMasterDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        MapCountryEndpoints(endpoints);
        MapCityEndpoints(endpoints);
        MapDistrictEndpoints(endpoints);
        MapBankEndpoints(endpoints);
        MapSymbolLookupEndpoints(endpoints);
    }

    #region Countries

    private static void MapCountryEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/master-data/countries")
            .WithTags("Master Data - Countries");

        // GET /api/master-data/countries
        group.MapGet("/", async (
            [AsParameters] MasterDataQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCountriesQuery
            {
                PageNumber = parameters.PageNumber ?? MasterDataQueryParameters.DefaultPageNumber,
                PageSize = parameters.PageSize ?? MasterDataQueryParameters.DefaultPageSize,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
                    ? MasterDataQueryParameters.DefaultSortBy
                    : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? MasterDataQueryParameters.DefaultSortDescending,
                SearchTerm = parameters.SearchTerm
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetCountries")
        .WithDescription("Get paginated list of countries");

        // POST /api/master-data/countries
        group.MapPost("/", async (
            [FromBody] CreateCountryCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/master-data/countries/{id}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("CreateCountry")
        .WithDescription("Create a new country");

        // PUT /api/master-data/countries/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateMasterDataRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCountryCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("UpdateCountry")
        .WithDescription("Update an existing country");

        // DELETE /api/master-data/countries/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteCountryCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataDelete)
        .WithName("DeleteCountry")
        .WithDescription("Soft-delete a country");

        // POST /api/master-data/countries/import
        group.MapPost("/import", async (
            IFormFile file,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            using var stream = file.OpenReadStream();
            var command = new ImportCountriesCommand { CsvStream = stream };
            var result = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<ImportResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .DisableAntiforgery()
        .WithName("ImportCountries")
        .WithDescription("Import countries from a CSV file (Code, Name). Upserts by Code.");
    }

    #endregion

    #region Cities

    private static void MapCityEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/master-data/cities")
            .WithTags("Master Data - Cities");

        // GET /api/master-data/cities
        group.MapGet("/", async (
            [AsParameters] MasterDataWithParentQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCitiesQuery
            {
                PageNumber = parameters.PageNumber ?? MasterDataQueryParameters.DefaultPageNumber,
                PageSize = parameters.PageSize ?? MasterDataQueryParameters.DefaultPageSize,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
                    ? MasterDataQueryParameters.DefaultSortBy
                    : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? MasterDataQueryParameters.DefaultSortDescending,
                SearchTerm = parameters.SearchTerm,
                CountryCode = parameters.ParentCode
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetCities")
        .WithDescription("Get paginated list of cities with optional country filter");

        // POST /api/master-data/cities
        group.MapPost("/", async (
            [FromBody] CreateCityCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/master-data/cities/{id}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("CreateCity")
        .WithDescription("Create a new city");

        // PUT /api/master-data/cities/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateChildMasterDataRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateCityCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                CountryCode = request.ParentCode
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("UpdateCity")
        .WithDescription("Update an existing city");

        // DELETE /api/master-data/cities/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteCityCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataDelete)
        .WithName("DeleteCity")
        .WithDescription("Soft-delete a city");

        // POST /api/master-data/cities/import
        group.MapPost("/import", async (
            IFormFile file,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            using var stream = file.OpenReadStream();
            var command = new ImportCitiesCommand { CsvStream = stream };
            var result = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<ImportResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .DisableAntiforgery()
        .WithName("ImportCities")
        .WithDescription("Import cities from a CSV file (Code, Name, CountryCode). Upserts by Code.");
    }

    #endregion

    #region Districts

    private static void MapDistrictEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/master-data/districts")
            .WithTags("Master Data - Districts");

        // GET /api/master-data/districts
        group.MapGet("/", async (
            [AsParameters] MasterDataWithParentQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetDistrictsQuery
            {
                PageNumber = parameters.PageNumber ?? MasterDataQueryParameters.DefaultPageNumber,
                PageSize = parameters.PageSize ?? MasterDataQueryParameters.DefaultPageSize,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
                    ? MasterDataQueryParameters.DefaultSortBy
                    : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? MasterDataQueryParameters.DefaultSortDescending,
                SearchTerm = parameters.SearchTerm,
                CityCode = parameters.ParentCode
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetDistricts")
        .WithDescription("Get paginated list of districts with optional city filter");

        // POST /api/master-data/districts
        group.MapPost("/", async (
            [FromBody] CreateDistrictCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/master-data/districts/{id}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("CreateDistrict")
        .WithDescription("Create a new district");

        // PUT /api/master-data/districts/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateChildMasterDataRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateDistrictCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                CityCode = request.ParentCode
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("UpdateDistrict")
        .WithDescription("Update an existing district");

        // DELETE /api/master-data/districts/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteDistrictCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataDelete)
        .WithName("DeleteDistrict")
        .WithDescription("Soft-delete a district");

        // POST /api/master-data/districts/import
        group.MapPost("/import", async (
            IFormFile file,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            using var stream = file.OpenReadStream();
            var command = new ImportDistrictsCommand { CsvStream = stream };
            var result = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<ImportResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .DisableAntiforgery()
        .WithName("ImportDistricts")
        .WithDescription("Import districts from a CSV file (Code, Name, CityCode). Upserts by Code.");
    }

    #endregion

    #region Banks

    private static void MapBankEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/master-data/banks")
            .WithTags("Master Data - Banks");

        // GET /api/master-data/banks
        group.MapGet("/", async (
            [AsParameters] MasterDataWithParentQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBanksQuery
            {
                PageNumber = parameters.PageNumber ?? MasterDataQueryParameters.DefaultPageNumber,
                PageSize = parameters.PageSize ?? MasterDataQueryParameters.DefaultPageSize,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
                    ? MasterDataQueryParameters.DefaultSortBy
                    : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? MasterDataQueryParameters.DefaultSortDescending,
                SearchTerm = parameters.SearchTerm,
                CountryCode = parameters.ParentCode
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("GetBanks")
        .WithDescription("Get paginated list of banks with optional country filter");

        // POST /api/master-data/banks
        group.MapPost("/", async (
            [FromBody] CreateBankCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/master-data/banks/{id}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("CreateBank")
        .WithDescription("Create a new bank");

        // PUT /api/master-data/banks/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateChildMasterDataRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateBankCommand
            {
                Id = id,
                Code = request.Code,
                Name = request.Name,
                CountryCode = request.ParentCode
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .WithName("UpdateBank")
        .WithDescription("Update an existing bank");

        // DELETE /api/master-data/banks/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteBankCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataDelete)
        .WithName("DeleteBank")
        .WithDescription("Soft-delete a bank");

        // POST /api/master-data/banks/import
        group.MapPost("/import", async (
            IFormFile file,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            using var stream = file.OpenReadStream();
            var command = new ImportBanksCommand { CsvStream = stream };
            var result = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<ImportResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataWrite)
        .DisableAntiforgery()
        .WithName("ImportBanks")
        .WithDescription("Import banks from a CSV file (Code, Name, CountryCode). Upserts by Code.");
    }

    #endregion

    #region Symbol Lookup

    private static void MapSymbolLookupEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/master-data/symbols")
            .WithTags("Master Data - Symbols");

        // GET /api/master-data/symbols/search (MUST come before /{symbol} to avoid route conflict)
        group.MapGet("/search", async (
            [FromQuery] string assetType,
            [FromQuery] string? search,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new Application.Features.ExternalData.MarketData.Queries.SearchSymbols.SearchSymbolsQuery(
                assetType, search, page, pageSize);

            var result = await mediator.Send(query, cancellationToken);
            var response = ApiResponse<Application.Features.ExternalData.MarketData.Queries.SearchSymbols.SymbolSearchResultDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("SearchSymbols")
        .WithDescription("Search available symbols for a given asset type (CryptoSpot or VnStock)");

        // GET /api/master-data/symbols/{symbol}
        group.MapGet("/{symbol}", async (
            string symbol,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var dto = await mediator.Send(new LookupSymbolQuery(symbol), cancellationToken);
            if (dto is null)
                return Results.NotFound(ApiResponse.ErrorResult("SYMBOL_NOT_FOUND", "Symbol not found."));

            var response = ApiResponse<SymbolLookupDto>.SuccessResult(dto);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.MasterDataRead)
        .WithName("LookupSymbol")
        .WithDescription("Resolve a symbol to its full name and asset type (VnStock or CryptoSpot)");

    }

    #endregion

}

#region Query Parameter Models

/// <summary>
/// Base query parameters for master data list endpoints.
/// </summary>
public sealed record MasterDataQueryParameters
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 10;
    public const string DefaultSortBy = "code";
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

    [FromQuery(Name = "search")]
    public string? SearchTerm { get; init; }
}

/// <summary>
/// Query parameters for child master data with parent filter.
/// </summary>
public sealed record MasterDataWithParentQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [DefaultValue(MasterDataQueryParameters.DefaultPageNumber)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(MasterDataQueryParameters.DefaultPageSize)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "sortBy")]
    [DefaultValue(MasterDataQueryParameters.DefaultSortBy)]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    [DefaultValue(MasterDataQueryParameters.DefaultSortDescending)]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "search")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "parentCode")]
    public string? ParentCode { get; init; }
}

#endregion

#region Request Models

/// <summary>
/// Request body for updating a root-level master data entity (Code + Name only).
/// </summary>
public sealed record UpdateMasterDataRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

/// <summary>
/// Request body for updating a child master data entity (Code + Name + ParentCode).
/// </summary>
public sealed record UpdateChildMasterDataRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string ParentCode { get; init; }
}

#endregion
