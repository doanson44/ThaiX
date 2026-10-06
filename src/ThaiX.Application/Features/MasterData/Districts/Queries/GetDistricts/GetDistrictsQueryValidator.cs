using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Districts.Queries.GetDistricts;

/// <summary>
/// Validator for GetDistrictsQuery.
/// </summary>
public sealed class GetDistrictsQueryValidator : AbstractValidator<GetDistrictsQuery>
{
    public GetDistrictsQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);

        RuleFor(x => x.CityCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CityCode))
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
