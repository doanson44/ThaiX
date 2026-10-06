# Build client Docker image (via Testcontainers), start container, run Playwright suite.
# Requires Docker Desktop. Do not set APP_BASE_URL unless attaching to an existing host.

param(
    [string] $Filter = "",
    [switch] $UpdateBaselines,
    [int] $Parallelism = 0
)

$ErrorActionPreference = "Stop"

if ($UpdateBaselines) {
    $env:UPDATE_BASELINES = "true"
}

if ($Parallelism -gt 0) {
    $env:VISUAL_TEST_PARALLELISM = "$Parallelism"
}

# Ensure fixture owns the Docker host (unless caller already set APP_BASE_URL).
if (-not $env:APP_BASE_URL) {
    Remove-Item Env:APP_BASE_URL -ErrorAction SilentlyContinue
}

$testArgs = @(
    "test",
    (Join-Path $PSScriptRoot "ThaiX.Client.VisualTests.csproj"),
    "--logger", "console;verbosity=normal"
)

if ($Filter) {
    $testArgs += @("--filter", $Filter)
}

try {
    & dotnet @testArgs
    exit $LASTEXITCODE
}
finally {
    if ($UpdateBaselines) {
        Remove-Item Env:UPDATE_BASELINES -ErrorAction SilentlyContinue
    }
}
