using System.Text.RegularExpressions;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Ensures <see cref="RouteCatalog"/> stays aligned with every <c>@page</c> in ThaiX.Client.
/// Runs without Docker / Playwright.
/// </summary>
public sealed class RouteCatalogCoverageTests
{
    private static readonly Regex PageDirective = new(
        @"@page\s+""(?<route>[^""]+)""",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [Fact]
    public void RouteCatalog_CoversEveryClientPageDirective_AndHasNoStaleEntries()
    {
        var clientPagesDir = Path.Combine(ResolveRepoRoot(), "src", "ThaiX.Client");
        Assert.True(Directory.Exists(clientPagesDir), $"Missing client project at {clientPagesDir}");

        var pageTemplates = Directory.EnumerateFiles(clientPagesDir, "*.razor", SearchOption.AllDirectories)
            .SelectMany(path => PageDirective.Matches(File.ReadAllText(path)).Select(m => m.Groups["route"].Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.NotEmpty(pageTemplates);

        var catalog = RouteCatalog.AllCatalogRoutes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = new List<string>();
        foreach (var template in pageTemplates)
        {
            if (!IsCovered(template, catalog))
            {
                missing.Add(template);
            }
        }

        var stale = new List<string>();
        foreach (var route in catalog)
        {
            if (!MatchesAnyTemplate(route, pageTemplates))
            {
                stale.Add(route);
            }
        }

        Assert.True(
            missing.Count == 0 && stale.Count == 0,
            "RouteCatalog is out of sync with ThaiX.Client @page directives." +
            (missing.Count > 0
                ? $"{Environment.NewLine}Missing coverage:{Environment.NewLine}  - " +
                  string.Join($"{Environment.NewLine}  - ", missing)
                : string.Empty) +
            (stale.Count > 0
                ? $"{Environment.NewLine}Stale catalog entries:{Environment.NewLine}  - " +
                  string.Join($"{Environment.NewLine}  - ", stale)
                : string.Empty));
    }

    private static bool IsCovered(string pageTemplate, HashSet<string> catalog)
    {
        if (!pageTemplate.Contains('{', StringComparison.Ordinal))
        {
            return catalog.Contains(pageTemplate);
        }

        // /blog/{Slug} → /blog/route-catalog-smoke-test (or any /blog/* beyond the list root)
        // /portfolio/{Id:guid} → /portfolio/{SmokeEntityId}
        // /admin/blog/posts/{Id:guid} → /admin/blog/posts/{SmokeEntityId}
        var parameterIndex = pageTemplate.IndexOf("/{", StringComparison.Ordinal);
        Assert.True(parameterIndex > 0, $"Unexpected parameterized route template: {pageTemplate}");
        var prefix = pageTemplate[..parameterIndex];

        return catalog.Any(route =>
            route.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(route, prefix, StringComparison.OrdinalIgnoreCase) &&
            IsConcreteParameterSegment(route[(prefix.Length + 1)..], pageTemplate));
    }

    private static bool IsConcreteParameterSegment(string remainder, string pageTemplate)
    {
        var firstSegment = remainder.Split('/', 2)[0];
        if (pageTemplate.Contains("{Id:guid}", StringComparison.OrdinalIgnoreCase) ||
            pageTemplate.Contains("{Id}", StringComparison.OrdinalIgnoreCase))
        {
            return Guid.TryParse(firstSegment, out _);
        }

        // {Slug} and similar string parameters — any non-empty concrete segment
        return !string.IsNullOrWhiteSpace(firstSegment) && !firstSegment.Contains('{', StringComparison.Ordinal);
    }

    private static bool MatchesAnyTemplate(string catalogRoute, IReadOnlyList<string> pageTemplates)
    {
        foreach (var template in pageTemplates)
        {
            if (!template.Contains('{', StringComparison.Ordinal))
            {
                if (string.Equals(template, catalogRoute, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                continue;
            }

            var parameterIndex = template.IndexOf("/{", StringComparison.Ordinal);
            if (parameterIndex <= 0)
            {
                continue;
            }

            var prefix = template[..parameterIndex];
            if (!catalogRoute.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var remainder = catalogRoute[(prefix.Length + 1)..];
            if (IsConcreteParameterSegment(remainder, template))
            {
                return true;
            }
        }

        return false;
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
