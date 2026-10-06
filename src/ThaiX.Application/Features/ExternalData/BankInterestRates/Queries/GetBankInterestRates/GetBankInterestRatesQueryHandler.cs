using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.BankInterestRates.Queries.GetBankInterestRates;

/// <summary>
/// Handler for GetBankInterestRatesQuery.
/// Retrieves bank interest rates from CafeF external API via IExternalDataService.
/// Caching, retry, timeout policies are applied automatically by infrastructure.
/// </summary>
public sealed class GetBankInterestRatesQueryHandler
    : IRequestHandler<GetBankInterestRatesQuery, BankInterestRatesResponse>
{
    private readonly IExternalDataService _externalDataService;

    public GetBankInterestRatesQueryHandler(IExternalDataService externalDataService)
    {
        _externalDataService = externalDataService;
    }

    public async Task<BankInterestRatesResponse> Handle(
        GetBankInterestRatesQuery request,
        CancellationToken cancellationToken)
    {
        // Get raw external data first
        var externalResult = await _externalDataService
            .GetBankInterestRatesAsync<ExternalBankInterestRatesResponse>(cancellationToken);

        if (externalResult is null)
        {
            return new BankInterestRatesResponse
            {
                Data = new List<BankInterestRateGridItemDto>(),
                Success = false,
                Message = "Failed to retrieve bank interest rates from external API."
            };
        }

        // Map external data to grid-friendly DTO
        var gridData = externalResult.Data?.Select(bank => new BankInterestRateGridItemDto
        {
            BankName = bank.Name,
            Symbol = bank.Symbol,
            IconUrl = bank.Icon,
            Month1 = bank.InterestRates?.FirstOrDefault(r => r.Deposit == 1)?.Value,
            Month3 = bank.InterestRates?.FirstOrDefault(r => r.Deposit == 3)?.Value,
            Month6 = bank.InterestRates?.FirstOrDefault(r => r.Deposit == 6)?.Value,
            Month9 = bank.InterestRates?.FirstOrDefault(r => r.Deposit == 9)?.Value,
            Month12 = bank.InterestRates?.FirstOrDefault(r => r.Deposit == 12)?.Value,
            Month18 = bank.InterestRates?.FirstOrDefault(r => r.Deposit == 18)?.Value,
            Month24 = bank.InterestRates?.FirstOrDefault(r => r.Deposit == 24)?.Value
        }).ToList() ?? new List<BankInterestRateGridItemDto>();

        return new BankInterestRatesResponse
        {
            Data = gridData,
            Message = externalResult.Message,
            Success = externalResult.Success
        };
    }
}
