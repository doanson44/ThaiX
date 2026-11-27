using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineFlumpExclusionTests
{
    private const string LibraryJson = """
    {
      "w": 800, "h": 480, "fps": 42, "bg": "#000000",
      "symbols": { "Root": 1, "Hidden": 2, "Shown": 3, "Fade": 4 },
      "chars": {
        "1": { "k": "sprite", "n": 1, "frames": [[
            { "o": "p", "d": 1, "id": 2, "n": "Hidden", "m": [1,0,0,1,10,10] },
            { "o": "p", "d": 2, "id": 3, "n": "Shown", "m": [1,0,0,1,20,20] },
            { "o": "p", "d": 3, "id": 4, "n": "Fade", "m": [1,0,0,1,0,0] }
        ]] },
        "2": { "k": "bitmap", "w": 8, "h": 8 },
        "3": { "k": "bitmap", "w": 16, "h": 16 },
        "4": { "k": "shape", "b": [0, 0, 800, 480], "f": [ { "s": { "t": "s", "c": [0,0,0,1] }, "d": "" } ] }
      }
    }
    """;

    [Fact]
    public void GetDrawsExcluding_SkipsNamedSubtrees()
    {
        var library = FrontlineFlumpLibrary.Parse(LibraryJson);
        var rootId = library.GetSymbolId("Root")!.Value;

        var all = library.GetDraws(rootId, 0);
        var filtered = library.GetDrawsExcluding(
            rootId,
            0,
            new HashSet<string>(StringComparer.Ordinal) { "Hidden", "Fade" });

        all.Should().HaveCount(3);
        filtered.Should().ContainSingle();
        filtered[0].BitmapId.Should().Be(3);
    }

    [Fact]
    public void GetDrawsExcluding_RemovesTheLegacyFrameControllerOverlay()
    {
        var libraryPath = FindLibraryJson();
        if (libraryPath is null)
            return;

        var library = FrontlineFlumpLibrary.Parse(File.ReadAllText(libraryPath));
        var gameId = library.GetSymbolId("Game")!.Value;

        var unfiltered = library.GetDraws(gameId, 0);
        var filtered = library.GetDrawsExcluding(
            gameId,
            0,
            new HashSet<string>(StringComparer.Ordinal) { "FrameController" });

        var opaqueLargeFills = filtered
            .Where(draw => draw.Kind == FrontlineFlumpDrawKind.Fill
                && draw.Alpha > 0
                && draw.MaxX - draw.MinX > 700
                && draw.MaxY - draw.MinY > 400)
            .Select(draw => (draw.BitmapId, draw.Alpha, draw.Red, draw.Green, draw.Blue))
            .ToArray();

        unfiltered.Should().Contain(draw => draw.Kind == FrontlineFlumpDrawKind.Fill
            && draw.Alpha > 0
            && draw.MaxX - draw.MinX > 700
            && draw.MaxY - draw.MinY > 400);
        opaqueLargeFills.Should().BeEmpty("the frame controller overlay must be excluded");
    }

    private static string? FindLibraryJson()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "ThaiX.Client",
                "wwwroot",
                "games",
                "frontline-tower-defense",
                "assets",
                "library.json");
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return null;
    }

    [Fact]
    public void GetDraws_RendersBitmapFilledShapesAsSprites()
    {
        const string json = """
        {
          "w": 800, "h": 480, "fps": 42, "bg": "#000000",
          "symbols": { "Root": 1 },
          "chars": {
            "1": { "k": "sprite", "n": 1, "frames": [[ { "o": "p", "d": 1, "id": 5, "m": [1,0,0,1,0,0] } ]] },
            "5": { "k": "shape", "b": [0, 0, 800, 480], "f": [
                { "s": { "t": "b", "id": 6, "m": [1,0,0,1,4,5] }, "d": "M0 0L800 0L800 480L0 480Z" }
            ] },
            "6": { "k": "bitmap", "w": 100, "h": 50 }
          }
        }
        """;

        var library = FrontlineFlumpLibrary.Parse(json);
        var draws = library.GetDraws(library.GetSymbolId("Root")!.Value, 0);

        draws.Should().ContainSingle();
        draws[0].Kind.Should().Be(FrontlineFlumpDrawKind.Bitmap);
        draws[0].BitmapId.Should().Be(6);
        draws[0].Matrix.Tx.Should().Be(4);
        draws[0].Matrix.Ty.Should().Be(5);
    }
}
