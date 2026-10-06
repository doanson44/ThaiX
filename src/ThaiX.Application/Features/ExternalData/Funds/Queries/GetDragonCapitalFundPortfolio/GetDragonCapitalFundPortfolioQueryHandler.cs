using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.Funds.Queries.GetDragonCapitalFundPortfolio;

/// <summary>
/// Handler for GetDragonCapitalFundPortfolioQuery.
/// Retrieves Dragon Capital fund portfolio data via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetDragonCapitalFundPortfolioQueryHandler
    : IRequestHandler<GetDragonCapitalFundPortfolioQuery, DragonCapitalFundPortfolioResponse>
{
    private readonly IExternalDataService _externalDataService;
    private static readonly string[] ValidFundCodes = { "VF1", "VF4", "VFMVN30", "VFMVND", "VFMMID" };

    public GetDragonCapitalFundPortfolioQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<DragonCapitalFundPortfolioResponse> Handle(
        GetDragonCapitalFundPortfolioQuery request,
        CancellationToken cancellationToken)
    {
        if (!ValidFundCodes.Contains(request.FundCode.ToUpperInvariant()))
        {
            return new DragonCapitalFundPortfolioResponse
            {
                FundCode = request.FundCode,
                Success = false,
                Message = $"Invalid fund code. Supported codes: {string.Join(", ", ValidFundCodes)}"
            };
        }

        var apiResponse = await _externalDataService
            .GetDragonCapitalFundPortfolioAsync<InternalDragonCapitalApiResponse>(
                request.FundCode.ToUpperInvariant(),
                cancellationToken);

        if (apiResponse?.ReturnValue is null)
        {
            return new DragonCapitalFundPortfolioResponse
            {
                FundCode = request.FundCode,
                Success = false,
                Message = "Failed to retrieve Dragon Capital fund portfolio from external API."
            };
        }

        var returnValue = apiResponse.ReturnValue;

        var assetTypeAllocations = returnValue.AllocationByAssetTypes?
            .Select(a => new AssetTypeAllocation
            {
                SourceName = a.SourceName ?? string.Empty,
                ValueAssetType = a.ValueAssetType
            })
            .ToList() ?? [];

        var sectorAllocations = returnValue.AllocationBySectors?
            .Select(s => new SectorAllocation
            {
                IndustryLevel2 = s.IndustryLevel2 ?? string.Empty,
                FundWeight = s.FundWeight?.GetValueOrDefault(request.FundCode.ToUpperInvariant(), 0) ?? 0
            })
            .ToList() ?? [];

        var top10Holdings = returnValue.Top10Holding?
            .Where(h => h.AssetId != "Total")
            .Select(h => new TopHoldingDto
            {
                AssetId = h.AssetId ?? string.Empty,
                Weight = h.Weight,
                Exchange = h.Exchange,
                IndustryLevel = h.IndustryLevel,
                SectorLevel = h.SectorLevel,
                HoldingVolume = h.HoldingVolume,
                MarketValue = h.MarketValue
            })
            .ToList() ?? [];

        return new DragonCapitalFundPortfolioResponse
        {
            FundCode = returnValue.FundCode ?? request.FundCode,
            TradingDate = returnValue.TradingDate,
            AllocationByAssetTypes = assetTypeAllocations,
            AllocationBySectors = sectorAllocations,
            Top10Holdings = top10Holdings,
            Success = true,
            Message = "Success"
        };
    }

    private sealed record InternalDragonCapitalApiResponse
    {
        public InternalReturnValue? ReturnValue { get; init; }
        public bool Cacheable { get; init; }
    }

    private sealed record InternalReturnValue
    {
        public string? FundCode { get; init; }
        public DateTime? TradingDate { get; init; }
        public List<InternalAssetTypeAllocation>? AllocationByAssetTypes { get; init; }
        public List<InternalSectorAllocation>? AllocationBySectors { get; init; }
        public List<InternalTopHolding>? Top10Holding { get; init; }
    }

    private sealed record InternalAssetTypeAllocation
    {
        public string? SourceName { get; init; }
        public decimal ValueAssetType { get; init; }
    }

    private sealed record InternalSectorAllocation
    {
        public string? IndustryLevel2 { get; init; }
        public Dictionary<string, decimal>? FundWeight { get; init; }
    }

    private sealed record InternalTopHolding
    {
        public string? AssetId { get; init; }
        public decimal Weight { get; init; }
        public string? Exchange { get; init; }
        public string? IndustryLevel { get; init; }
        public string? SectorLevel { get; init; }
        public long? HoldingVolume { get; init; }
        public decimal? MarketValue { get; init; }
    }
}
