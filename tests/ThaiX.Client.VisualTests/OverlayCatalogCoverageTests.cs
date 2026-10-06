using System.Text.RegularExpressions;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Ensures every dialog-style Razor component under ThaiX.Client is listed in <see cref="OverlayCatalog"/>.
/// </summary>
public sealed class OverlayCatalogCoverageTests
{
    private static readonly Regex OpenAsyncComponent = new(
        @"DialogService\.OpenAsync\s*<\s*(?<name>[A-Za-z0-9_]+)\s*>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Fact]
    public void OverlayCatalog_CoversEveryOpenAsyncDialogComponent()
    {
        var clientRoot = Path.Combine(ResolveRepoRoot(), "src", "ThaiX.Client");
        Assert.True(Directory.Exists(clientRoot), $"Missing client at {clientRoot}");

        var openedComponents = Directory.EnumerateFiles(clientRoot, "*.razor", SearchOption.AllDirectories)
            .SelectMany(path => OpenAsyncComponent.Matches(File.ReadAllText(path))
                .Select(m => m.Groups["name"].Value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(openedComponents);

        var catalog = OverlayCatalog.RequiredComponentNames.ToHashSet(StringComparer.Ordinal);
        var missing = openedComponents.Where(name => !catalog.Contains(name)).ToArray();
        var stale = catalog.Where(name => !openedComponents.Contains(name, StringComparer.Ordinal)).ToArray();

        // DonateModal is opened as DonateModal; Confirm* catalog entries are not OpenAsync form dialogs.
        Assert.True(
            missing.Length == 0 && stale.Length == 0,
            "OverlayCatalog form dialogs are out of sync with DialogService.OpenAsync<T> usages." +
            (missing.Length > 0
                ? $"{Environment.NewLine}Missing from catalog:{Environment.NewLine}  - " +
                  string.Join($"{Environment.NewLine}  - ", missing)
                : string.Empty) +
            (stale.Length > 0
                ? $"{Environment.NewLine}Stale catalog form dialogs:{Environment.NewLine}  - " +
                  string.Join($"{Environment.NewLine}  - ", stale)
                : string.Empty));
    }

    private static string ResolveRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ThaiX.sln")) &&
                File.Exists(Path.Combine(dir.FullName, "src", "ThaiX.Client", "ThaiX.Client.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repo root (ThaiX.sln + src/ThaiX.Client) from the test output directory.");
    }
}
