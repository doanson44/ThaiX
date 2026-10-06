using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Queries.ValidateJsonBinCode;

public sealed class ValidateJsonBinCodeQueryValidator : AbstractValidator<ValidateJsonBinCodeQuery>
{
    public ValidateJsonBinCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(JsonBin.CodeMaxLength)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Matches(@"^[A-Za-z0-9._-]+$")
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
