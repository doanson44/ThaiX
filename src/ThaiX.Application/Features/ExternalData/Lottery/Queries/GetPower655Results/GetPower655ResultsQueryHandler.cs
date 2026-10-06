using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.Lottery.Queries.GetPower655Results;

/// <summary>
/// Handles GetPower655ResultsQuery by delegating to the external data service.
/// </summary>
public sealed class GetPower655ResultsQueryHandler
    : IRequestHandler<GetPower655ResultsQuery, Power655ResultsResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetPower655ResultsQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<Power655ResultsResponse> Handle(GetPower655ResultsQuery request, CancellationToken cancellationToken)
    {
        var results = await _externalDataService.GetPower655ResultsAsync(
            request.DateFrom,
            request.DateTo,
            cancellationToken);

        return new Power655ResultsResponse
        {
            Results = results ?? [],
            TotalCount = results?.Count ?? 0
        };
    }
}
