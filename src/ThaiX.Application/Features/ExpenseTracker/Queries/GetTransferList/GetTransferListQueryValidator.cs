using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferList;

/// <summary>
/// Validator for GetTransferListQuery.
/// </summary>
public sealed class GetTransferListQueryValidator : AbstractValidator<GetTransferListQuery>
{
    public GetTransferListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
