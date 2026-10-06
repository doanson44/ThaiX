using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagList;

/// <summary>
/// Validator for GetTransactionTagListQuery.
/// </summary>
public sealed class GetTransactionTagListQueryValidator : AbstractValidator<GetTransactionTagListQuery>
{
    public GetTransactionTagListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
