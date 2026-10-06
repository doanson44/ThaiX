using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletList;

/// <summary>
/// Validator for GetWalletListQuery.
/// </summary>
public sealed class GetWalletListQueryValidator : AbstractValidator<GetWalletListQuery>
{
    public GetWalletListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
