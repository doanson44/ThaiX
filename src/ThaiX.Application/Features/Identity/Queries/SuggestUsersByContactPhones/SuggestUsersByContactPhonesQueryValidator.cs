using FluentValidation;

namespace ThaiX.Application.Features.Identity.Queries.SuggestUsersByContactPhones;

public sealed class SuggestUsersByContactPhonesQueryValidator : AbstractValidator<SuggestUsersByContactPhonesQuery>
{
    public SuggestUsersByContactPhonesQueryValidator()
    {
        RuleFor(x => x.ContactId).NotEmpty();
    }
}
