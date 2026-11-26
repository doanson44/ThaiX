using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineFlumpTextLayoutTests
{
    private static readonly FrontlineFlumpFont Font = new(
        "Test",
        ["A", "B", " "],
        ['A', 'B', ' '],
        [1024, 512, 256],
        ascent: 1024,
        descent: 0,
        leading: 0);

    [Fact]
    public void GetPaints_ScalesAdvancesByTheLegacyFontUnitSize()
    {
        var placement = Field(text: "AB", size: 512);

        var paints = FrontlineFlumpTextLayout.GetPaints(placement, _ => Font);

        paints.Should().HaveCount(2);
        paints[0].X.Should().BeApproximately(2, 0.001);
        paints[0].Scale.Should().BeApproximately(0.5, 0.0001);
        paints[1].X.Should().BeApproximately(2 + 512, 0.001);
    }

    [Fact]
    public void GetPaints_WithScaleFactor_IncreasesGlyphSizeAndSpacing()
    {
        var placement = Field(text: "AB", size: 512);

        var paints = FrontlineFlumpTextLayout.GetPaints(placement, _ => Font, scaleFactor: 1.2);

        paints.Should().HaveCount(2);
        paints[0].Scale.Should().BeApproximately(0.6, 0.0001);
        paints[1].X.Should().BeApproximately(2 + 512 * 1.2, 0.001);
    }

    [Fact]
    public void GetPaints_WhenAGlyphIsMissing_AdvancesByTheLegacyFallbackWidth()
    {
        var placement = Field(text: "AZ", size: 100);

        var paints = FrontlineFlumpTextLayout.GetPaints(placement, _ => Font);

        // 'Z' is not in the font, so it consumes size * 0.3 and is not painted.
        paints.Should().ContainSingle();
        paints[0].GlyphIndex.Should().Be(0);
    }

    [Fact]
    public void GetPaints_WithWordWrap_MovesBlockedWordsToTheNextLineBaseline()
    {
        var placement = Field(text: "A B", size: 512, width: 700, wordWrap: true);

        var paints = FrontlineFlumpTextLayout.GetPaints(placement, _ => Font);

        paints.Should().HaveCount(2);
        paints[0].Baseline.Should().BeApproximately(514, 0.001);
        paints[1].Baseline.Should().BeApproximately(514 + 512, 0.001);
        paints[1].X.Should().BeApproximately(2, 0.001);
    }

    [Fact]
    public void GetPaints_WithRightAlignment_OffsetsTheLineByItsMeasuredWidth()
    {
        var placement = Field(text: "A", size: 100, width: 400, align: 1);

        var paints = FrontlineFlumpTextLayout.GetPaints(placement, _ => Font);

        // Box width 400, two pixel inset, glyph advance 100 -> x = 2 + (396 - 100).
        paints[0].X.Should().BeApproximately(298, 0.001);
    }

    private static FrontlineFlumpTextPlacement Field(
        string text,
        double size,
        double width = 200,
        int align = 0,
        bool wordWrap = false) =>
        new(
            "field",
            true,
            FrontlineFlumpMatrix.Identity,
            MinX: 0,
            MinY: 0,
            MaxX: width,
            MaxY: 40,
            FontId: 1,
            Size: size,
            Red: 255,
            Green: 255,
            Blue: 255,
            Alpha: 255,
            Align: align,
            Leading: 0,
            LeftMargin: 0,
            RightMargin: 0,
            Indent: 0,
            WordWrap: wordWrap,
            Multiline: false,
            Border: false,
            Text: text,
            Runs: []);
}
