using MediatR;
using ThaiX.Application.Common.Enums;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.Banking.Queries.GetVnExpressBankRates;

/// <summary>
/// Handler for GetVnExpressBankRatesQuery.
/// Calls the VnExpress endpoint matching the requested channel and maps its rows to grid items.
/// Returns Success=false when the channel cannot be loaded.
/// </summary>
public sealed class GetVnExpressBankRatesQueryHandler
    : IRequestHandler<GetVnExpressBankRatesQuery, VnExpressBankRatesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetVnExpressBankRatesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<VnExpressBankRatesResponse> Handle(
        GetVnExpressBankRatesQuery request,
        CancellationToken cancellationToken)
    {
        var apiResponse = await _externalDataService
            .GetVnExpressBankRatesAsync<VnExpressBankRatesApiResponse>(request.Channel, cancellationToken)
            .ConfigureAwait(false);

        var data = SelectChannelRows(apiResponse, request.Channel)
            .Where(row => !string.IsNullOrWhiteSpace(row.Bank))
            .Select(row => new VnExpressBankRateItemDto
            {
                BankName = row.Bank!,
                LogoUrl = BlankToNull(row.Logo1),
                Note = BlankToNull(row.Note),
                UpdatedAt = ToTimestamp(row.UpdatedAt),
                Month1 = Normalize(row.Rate1),
                Month3 = Normalize(row.Rate3),
                Month6 = Normalize(row.Rate6),
                Month9 = Normalize(row.Rate9),
                Month12 = Normalize(row.Rate12)
            })
            .ToList();

        if (data.Count == 0)
        {
            return new VnExpressBankRatesResponse
            {
                Channel = request.Channel,
                Success = false,
                Message = $"Failed to retrieve {request.Channel.ToString().ToLowerInvariant()} bank deposit rates from VnExpress."
            };
        }

        return new VnExpressBankRatesResponse
        {
            Channel = request.Channel,
            Data = data,
            Success = true,
            Message = "Success"
        };
    }

    /// <summary>Picks the payload belonging to the requested channel; both channels share one response shape.</summary>
    private static List<VnExpressBankRateRow> SelectChannelRows(
        VnExpressBankRatesApiResponse? apiResponse,
        BankRateChannel channel)
    {
        if (apiResponse?.Data is null)
        {
            return [];
        }

        return channel == BankRateChannel.Offline
            ? apiResponse.Data.BankRateOffline ?? []
            : apiResponse.Data.BankRateOnline ?? [];
    }

    /// <summary>The source reports 0 for terms it does not offer; treat those as unavailable.</summary>
    private static decimal? Normalize(decimal rate) => rate > 0 ? rate : null;

    private static DateTimeOffset? ToTimestamp(long? unixSeconds) =>
        unixSeconds is > 0 ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value) : null;

    private static string? BlankToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
