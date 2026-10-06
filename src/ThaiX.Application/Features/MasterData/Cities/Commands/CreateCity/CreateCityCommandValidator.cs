using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.CreateCity;

/// <summary>
/// Validator for CreateCityCommand.
/// </summary>
public sealed class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
{
    public CreateCityCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(20)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(256)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.CountryCode)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(10)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
