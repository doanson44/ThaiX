using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Cities.Queries.GetCities;

/// <summary>
/// Validator for GetCitiesQuery.
/// </summary>
public sealed class GetCitiesQueryValidator : AbstractValidator<GetCitiesQuery>
{
    public GetCitiesQueryValidator()
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
