using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithErrorCode(ErrorCodes.INVALID_FORMAT)
                .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH);
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{9,15}$")
                .WithErrorCode(ErrorCodes.INVALID_FORMAT);
        });

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) ||
                      !string.IsNullOrWhiteSpace(x.PhoneNumber) ||
                      x.EmailConfirmed.HasValue ||
                      x.TwoFactorEnabled.HasValue)
            .WithErrorCode(ErrorCodes.INVALID_REQUEST);
    }
}
