using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Countries.Queries.GetCountries;

/// <summary>
/// Validator for GetCountriesQuery.
/// </summary>
public sealed class GetCountriesQueryValidator : AbstractValidator<GetCountriesQuery>
{
    public GetCountriesQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
