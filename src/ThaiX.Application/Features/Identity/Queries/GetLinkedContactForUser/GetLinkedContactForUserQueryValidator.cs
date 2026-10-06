using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Identity.Queries.GetLinkedContactForUser;

public sealed class GetLinkedContactForUserQueryValidator : AbstractValidator<GetLinkedContactForUserQuery>
{
    public GetLinkedContactForUserQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
