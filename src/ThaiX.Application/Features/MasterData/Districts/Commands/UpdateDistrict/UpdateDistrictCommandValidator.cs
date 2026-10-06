using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.UpdateDistrict;

/// <summary>
/// Validator for UpdateDistrictCommand.
/// </summary>
public sealed class UpdateDistrictCommandValidator : AbstractValidator<UpdateDistrictCommand>
{
    public UpdateDistrictCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

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
