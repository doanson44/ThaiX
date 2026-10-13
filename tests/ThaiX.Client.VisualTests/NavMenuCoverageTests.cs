using System.Text.RegularExpressions;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Ensures NavMenu keeps a public Blog entry pointing at <c>/blog</c>.
/// Runs without Docker / Playwright.
/// </summary>
public sealed class NavMenuCoverageTests
{
    private static readonly Regex BlogPath = new(
        @"Path\s*=\s*""blog""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Fact]
    public void NavMenu_IncludesPublicBlogListPath()
    {
        var navMenuPath = Path.Combine(RepoRoot.Resolve(), "src", "ThaiX.Client", "Layout", "NavMenu.razor");
        Assert.True(File.Exists(navMenuPath), $"Missing NavMenu at {navMenuPath}");

        var source = File.ReadAllText(navMenuPath);
        var matches = BlogPath.Matches(source);

        Assert.True(
            matches.Count >= 1,
            "NavMenu.razor must include at least one Path=\"blog\" entry for the public Blog list.");
    }
}
