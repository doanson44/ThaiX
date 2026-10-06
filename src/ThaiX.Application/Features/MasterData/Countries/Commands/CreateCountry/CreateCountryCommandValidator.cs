using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.CreateCountry;

/// <summary>
/// Validator for CreateCountryCommand.
/// </summary>
public sealed class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(10)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(256)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
