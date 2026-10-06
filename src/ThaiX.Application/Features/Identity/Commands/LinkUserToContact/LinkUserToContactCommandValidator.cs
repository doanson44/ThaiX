using FluentValidation;

namespace ThaiX.Application.Features.Identity.Commands.LinkUserToContact;

public sealed class LinkUserToContactCommandValidator : AbstractValidator<LinkUserToContactCommand>
{
    public LinkUserToContactCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ContactId).NotEmpty();
    }
}
