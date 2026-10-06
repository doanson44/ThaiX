using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Authentication.Commands.Login;

/// <summary>
/// Validator for LoginCommand.
/// </summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
