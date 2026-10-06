using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Common.Validators;

/// <summary>
/// Validator for PagedRequest to ensure valid pagination parameters.
/// </summary>
public sealed class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    public PagedRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithErrorCode(ErrorCodes.INVALID_PAGE_NUMBER);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(PagedRequest.MinPageSize, PagedRequest.MaxPageSize)
            .WithErrorCode(ErrorCodes.INVALID_PAGE_SIZE);

        RuleFor(x => x.SortBy)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithErrorCode(ErrorCodes.INVALID_SORT_FIELD);
    }
}
