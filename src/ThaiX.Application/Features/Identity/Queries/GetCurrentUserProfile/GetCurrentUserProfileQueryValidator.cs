using FluentValidation;

namespace ThaiX.Application.Features.Identity.Queries.GetCurrentUserProfile;

public sealed class GetCurrentUserProfileQueryValidator : AbstractValidator<GetCurrentUserProfileQuery>
{
    public GetCurrentUserProfileQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
