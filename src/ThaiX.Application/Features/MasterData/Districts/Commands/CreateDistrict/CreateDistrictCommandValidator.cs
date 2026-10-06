using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.CreateDistrict;

/// <summary>
/// Validator for CreateDistrictCommand.
/// </summary>
public sealed class CreateDistrictCommandValidator : AbstractValidator<CreateDistrictCommand>
{
    public CreateDistrictCommandValidator()
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

        RuleFor(x => x.CityCode)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(20)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
