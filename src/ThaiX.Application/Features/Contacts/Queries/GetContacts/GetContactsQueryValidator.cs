using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Queries.GetContacts;

/// <summary>
/// Validator for GetContactsQuery.
/// </summary>
public sealed class GetContactsQueryValidator : AbstractValidator<GetContactsQuery>
{
    public GetContactsQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);

        RuleFor(x => x.Tag)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Tag))
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
