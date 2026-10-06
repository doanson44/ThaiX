using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Queries.GetSharedJsonBin;

public sealed class GetSharedJsonBinQueryValidator : AbstractValidator<GetSharedJsonBinQuery>
{
    public GetSharedJsonBinQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(JsonBin.ShareTokenMaxLength)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Matches(@"^[A-Fa-f0-9]+$")
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
