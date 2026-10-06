using ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.UnitTests.Features.JsonBins;

public sealed class CreateJsonBinCommandValidatorTests
{
    private readonly CreateJsonBinCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenCodeOmitted()
    {
        var result = _sut.Validate(new CreateJsonBinCommand
        {
            Name = "ok",
            ContentJson = "{}"
        });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenCodeHasInvalidCharacters()
    {
        var result = _sut.Validate(new CreateJsonBinCommand
        {
            Code = "bad code!",
            Name = "ok",
            ContentJson = "{}"
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateJsonBinCommand.Code));
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenRequiredFieldsPresent()
    {
        var result = _sut.Validate(new CreateJsonBinCommand
        {
            Code = "CFG_MAIN",
            Name = "ok",
            Category = JsonBinCategories.Cache,
            ContentJson = "{\"v\":1}"
        });

        result.IsValid.Should().BeTrue();
    }
}
