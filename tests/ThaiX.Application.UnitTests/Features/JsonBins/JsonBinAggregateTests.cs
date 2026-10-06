using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.UnitTests.Features.JsonBins;

public sealed class JsonBinAggregateTests
{
    [Fact]
    public void Create_ShouldNormalizeCodeAndSetMetadata()
    {
        var content = "{\"a\":1}"u8.ToArray();
        var bin = JsonBin.Create(" demo-code ", "demo", JsonBinCategories.Config, content, tags: "t1");

        bin.Code.Should().Be("DEMO-CODE");
        bin.Name.Should().Be("demo");
        bin.Category.Should().Be(JsonBinCategories.Config);
        bin.SizeBytes.Should().Be(content.LongLength);
        bin.IsCompressed.Should().BeFalse();
        bin.Tags.Should().Be("t1");
    }

    [Fact]
    public void NormalizeCode_ShouldRejectEmpty()
    {
        var act = () => JsonBin.NormalizeCode(" ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NormalizeCodeOrNull_ShouldReturnNull_WhenBlank()
    {
        JsonBin.NormalizeCodeOrNull(" ").Should().BeNull();
        JsonBin.NormalizeCodeOrNull(null).Should().BeNull();
    }

    [Fact]
    public void GenerateCode_ShouldIncludeCategoryToken()
    {
        var code = JsonBin.GenerateCode(JsonBinCategories.Config);

        code.Should().StartWith("CONFIG_");
        code.Length.Should().BeLessThanOrEqualTo(JsonBin.CodeMaxLength);
    }

    [Fact]
    public void ChangeCode_ShouldNormalize()
    {
        var bin = JsonBin.Create("OLD", "demo", JsonBinCategories.Temp, "{}"u8.ToArray());

        bin.ChangeCode(" new.code ");

        bin.Code.Should().Be("NEW.CODE");
    }

    [Fact]
    public void Rename_ShouldRejectEmpty()
    {
        var bin = JsonBin.Create("CODE1", "demo", JsonBinCategories.Temp, "{}"u8.ToArray());

        var act = () => bin.Rename(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReplaceContent_ShouldUpdateSizeAndFlags()
    {
        var bin = JsonBin.Create("CODE1", "demo", JsonBinCategories.Temp, "{}"u8.ToArray());
        var next = "{\"x\":true}"u8.ToArray();

        bin.ReplaceContent(next, "application/json", isCompressed: true);

        bin.IsCompressed.Should().BeTrue();
        bin.SizeBytes.Should().Be(next.LongLength);
    }

    [Fact]
    public void SoftDelete_ShouldMarkDeleted()
    {
        var bin = JsonBin.Create("CODE1", "demo", JsonBinCategories.Temp, "{}"u8.ToArray());

        bin.SoftDelete();

        bin.IsDeleted.Should().BeTrue();
        bin.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkExpired_ShouldSetExpiredAtUtc()
    {
        var bin = JsonBin.Create("CODE1", "demo", JsonBinCategories.Temp, "{}"u8.ToArray());

        bin.MarkExpired();

        bin.ExpiredAtUtc.Should().NotBeNull();
        bin.ExpiredAtUtc.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void ChangeExpiration_ShouldAllowClear()
    {
        var bin = JsonBin.Create(
            "CODE1",
            "demo",
            JsonBinCategories.Temp,
            "{}"u8.ToArray(),
            expiredAtUtc: DateTime.UtcNow.AddDays(1));

        bin.ChangeExpiration(null);

        bin.ExpiredAtUtc.Should().BeNull();
    }

    [Fact]
    public void EnableShare_ShouldSetActiveToken()
    {
        var bin = JsonBin.Create("CODE1", "demo", JsonBinCategories.Temp, "{}"u8.ToArray());

        var token = bin.EnableShare(DateTime.UtcNow.AddHours(1));

        token.Should().NotBeNullOrWhiteSpace();
        bin.IsShareActive(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_ShouldRevokeShare()
    {
        var bin = JsonBin.Create("CODE1", "demo", JsonBinCategories.Temp, "{}"u8.ToArray());
        bin.EnableShare();

        bin.SoftDelete();

        bin.ShareToken.Should().BeNull();
        bin.IsShareActive(DateTime.UtcNow).Should().BeFalse();
    }
}
