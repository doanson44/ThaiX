using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Authentication.Commands.ClientCredentials;

/// <summary>
/// Validator for ClientCredentialsCommand.
/// </summary>
public sealed class ClientCredentialsCommandValidator : AbstractValidator<ClientCredentialsCommand>
{
    public ClientCredentialsCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.ClientSecret)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
