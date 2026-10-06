using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Banks.Queries.GetBanks;

/// <summary>
/// Validator for GetBanksQuery.
/// </summary>
public sealed class GetBanksQueryValidator : AbstractValidator<GetBanksQuery>
{
    public GetBanksQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);

        RuleFor(x => x.CountryCode)
            .MaximumLength(10)
            .When(x => !string.IsNullOrWhiteSpace(x.CountryCode))
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
