namespace ThaiX.Client.VisualTests;

/// <summary>
/// Locates the repository root by walking up from the test output directory.
/// Accepts either <c>ThaiX.sln</c> or <c>ThaiX.slnx</c>.
/// </summary>
internal static class RepoRoot
{
    public static string Resolve()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if ((File.Exists(Path.Combine(dir.FullName, "ThaiX.sln")) ||
                 File.Exists(Path.Combine(dir.FullName, "ThaiX.slnx"))) &&
                File.Exists(Path.Combine(dir.FullName, "src", "ThaiX.Client", "ThaiX.Client.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repo root (ThaiX.sln/ThaiX.slnx + src/ThaiX.Client) from the test output directory.");
    }
}
