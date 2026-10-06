using FluentValidation;
using ThaiX.Application.Common.Constants;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Commands.UpdateJsonBin;

public sealed class UpdateJsonBinCommandValidator : AbstractValidator<UpdateJsonBinCommand>
{
    public UpdateJsonBinCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Code)
            .MaximumLength(JsonBin.CodeMaxLength)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Matches(@"^[A-Za-z0-9._-]+$")
            .WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .When(x => !string.IsNullOrWhiteSpace(x.Code));

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Category)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Tags)
            .MaximumLength(500)
            .When(x => x.Tags is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
