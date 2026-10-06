using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Users.Queries.GetUsers;

/// <summary>
/// Validator for GetUsersQuery.
/// Inherits base pagination validation and adds query-specific rules.
/// </summary>
public sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        // Include base pagination validation
        Include(new Common.Validators.PagedRequestValidator());

        // Query-specific validation
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
