using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineFlumpLibraryTests
{
    private const string SampleLibrary = """
    {
      "chars": {
        "1": {"k": "bitmap", "w": 10, "h": 20},
        "2": {"k": "shape", "b": [0, 0, 10, 20], "img": 1, "m": [1, 0, 0, 1, 0, 0]},
        "3": {"k": "sprite", "frames": [[{"o": "p", "d": 1, "id": 2, "m": [1, 0, 0, 1, 5, 7]}]], "n": 1},
        "4": {"k": "bitmap", "w": 4, "h": 4},
        "5": {"k": "shape", "b": [0, 0, 4, 4], "img": 4, "m": [1, 0, 0, 1, 0, 0]},
        "6": {"k": "sprite", "frames": [
          [{"o": "p", "d": 1, "id": 5, "m": [1, 0, 0, 1, 0, 0]}, {"o": "p", "d": 2, "id": 5, "m": [1, 0, 0, 1, 9, 9]}],
          [{"o": "r", "d": 1}, {"o": "p", "d": 2, "id": 2, "m": [1, 0, 0, 1, 3, 3]}]
        ], "n": 2},
        "7": {"k": "shape", "b": [-16, -16, 16, 16], "f": [{"s": {"t": "s", "c": [62, 55, 40, 1]}, "d": "M-5.35 -16L5.35 -16Q16 -16 16 -5.35L16 5.35Q16 16 5.35 16L-5.35 16Q-16 16 -16 5.35L-16 -5.35Q-16 -16 -5.35 -16"}], "l": []}
      },
      "symbols": {"testSprite": 3, "testAnim": 6, "testTile": 7}
    }
    """;

    [Fact]
    public void GetDraws_ComposesPlacementMatrixOntoBitmap()
    {
        var library = FrontlineFlumpLibrary.Parse(SampleLibrary);

        var draws = library.GetDraws(library.GetSymbolId("testSprite")!.Value, 0);

        draws.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Kind = FrontlineFlumpDrawKind.Bitmap,
                BitmapId = 1,
                Width = 10.0,
                Height = 20.0,
                Matrix = new FrontlineFlumpMatrix(1, 0, 0, 1, 5, 7)
            });
    }

    [Fact]
    public void GetTextPlacements_AppliesTextCharacterMatrixToNestedPlacement()
    {
        const string json = """
        {
          "chars": {
            "1": {"k": "text", "b": [0, 0, 10, 10], "m": [1, 0, 0, 1, -5, 2], "r": []},
            "2": {"k": "sprite", "frames": [[
              {"o": "p", "d": 1, "id": 1, "m": [1, 0, 0, 1, 20, 30]}
            ]], "n": 1}
          },
          "symbols": {"root": 2}
        }
        """;
        var library = FrontlineFlumpLibrary.Parse(json);

        var placements = library.GetTextPlacements(library.GetSymbolId("root")!.Value, 0);

        placements.Should().ContainSingle()
            .Which.Matrix.Should().Be(new FrontlineFlumpMatrix(1, 0, 0, 1, 15, 32));
    }

    [Fact]
    public void GetDraws_AppliesRemoveOperationsAcrossFrames()
    {
        var library = FrontlineFlumpLibrary.Parse(SampleLibrary);

        var draws = library.GetDraws(library.GetSymbolId("testAnim")!.Value, 1);

        draws.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Kind = FrontlineFlumpDrawKind.Bitmap,
                BitmapId = 1,
                Matrix = new FrontlineFlumpMatrix(1, 0, 0, 1, 3, 3)
            });
    }

    [Fact]
    public void GetDraws_ExtractsVectorFillColorAndCornerRadius()
    {
        var library = FrontlineFlumpLibrary.Parse(SampleLibrary);

        var draws = library.GetDraws(library.GetSymbolId("testTile")!.Value, 0);

        draws.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Kind = FrontlineFlumpDrawKind.Fill,
                MinX = -16.0,
                MinY = -16.0,
                MaxX = 16.0,
                MaxY = 16.0,
                Red = (byte)62,
                Green = (byte)55,
                Blue = (byte)40,
                Alpha = (byte)255
            });

        draws[0].CornerRadius.Should().BeApproximately(10.65, 0.01);
    }

    [Fact]
    public void GetPlacements_ReturnsNamedChildrenInDepthOrder()
    {
        const string json = """
        {
          "chars": {
            "1": {"k": "bitmap", "w": 2, "h": 2},
            "2": {"k": "shape", "b": [0, 0, 2, 2], "img": 1, "m": [1, 0, 0, 1, 0, 0]},
            "3": {"k": "sprite", "frames": [[
              {"o": "p", "d": 1, "id": 2, "m": [1, 0, 0, 1, 0, 0], "n": "turret"},
              {"o": "p", "d": 3, "id": 2, "m": [1, 0, 0, 1, 4, 4]}
            ]], "n": 1}
          },
          "symbols": {"root": 3}
        }
        """;
        var library = FrontlineFlumpLibrary.Parse(json);

        var placements = library.GetPlacements(library.GetSymbolId("root")!.Value, 0);

        placements.Should().HaveCount(2);
        placements[0].Name.Should().Be("turret");
        placements[1].Name.Should().BeNull();
    }

    [Fact]
    public void GetDraws_AppliesMoveOperationSwapAcrossFrames()
    {
        const string json = """
        {
          "chars": {
            "1": {"k": "bitmap", "w": 10, "h": 20},
            "2": {"k": "shape", "b": [0, 0, 10, 20], "img": 1, "m": [1, 0, 0, 1, 0, 0]},
            "4": {"k": "bitmap", "w": 4, "h": 4},
            "5": {"k": "shape", "b": [0, 0, 4, 4], "img": 4, "m": [1, 0, 0, 1, 0, 0]},
            "8": {"k": "sprite", "frames": [
              [{"o": "p", "d": 1, "id": 2, "m": [1, 0, 0, 1, 0, 0]}],
              [{"o": "p", "d": 1, "mv": 1, "id": 5}]
            ], "n": 2}
          },
          "symbols": {"testSwap": 8}
        }
        """;
        var library = FrontlineFlumpLibrary.Parse(json);

        var draws = library.GetDraws(library.GetSymbolId("testSwap")!.Value, 1);

        draws.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                Kind = FrontlineFlumpDrawKind.Bitmap,
                BitmapId = 4,
                Width = 4.0,
                Height = 4.0
            });
    }
}
