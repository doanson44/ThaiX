# =============================================================================
# verify-phase.ps1 - ThaiX Client Refactor Verification (Layer 1)
# =============================================================================
# Checks: Build, @using static ResourceKeys, L["key"] literals, fixed px widths, resx sync.
#
# Usage:
#   .\verify-phase.ps1 -Phase 1
#   .\verify-phase.ps1 -Phase 2 -ChangedFiles "Layout/MainLayout.razor,Layout/NavMenu.razor"
#   .\verify-phase.ps1 -Phase 12 -ChangedFiles $fileList -NoBuild
# =============================================================================
param(
    [Parameter(Mandatory)]
    [int]$Phase,

    [string]$ChangedFiles = "",

    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"
$repoRoot = $PSScriptRoot | Split-Path -Parent
$clientRoot = Join-Path $repoRoot "src/ThaiX.Client"

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  THAIX VERIFY - Phase $Phase" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$passed = 0
$warned = 0
$failed = 0

# ------------------------------------------------------------------
# 1. BUILD
# ------------------------------------------------------------------
if (-not $NoBuild) {
    Write-Host ">>> STEP 1: Build ThaiX.Client" -ForegroundColor White

    $projPath = Join-Path $clientRoot "ThaiX.Client.csproj"
    $buildOutput = & dotnet build $projPath --no-restore 2>&1
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host "  PASS  Build succeeded" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "  FAIL  Build failed with exit code $exitCode" -ForegroundColor Red
        $buildOutput | Where-Object { $_ -match "error CS" } | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Red
        }
        $failed++
        exit 1
    }
} else {
    Write-Host ">>> STEP 1: Build SKIPPED [-NoBuild]" -ForegroundColor DarkGray
}

# ------------------------------------------------------------------
# 2. STATIC ANTI-PATTERN SCAN
# ------------------------------------------------------------------
Write-Host ""
Write-Host ">>> STEP 2: Anti-Pattern Scan" -ForegroundColor White

$scanFiles = @()
if ($ChangedFiles) {
    $paths = $ChangedFiles -split "," | ForEach-Object { $_.Trim() } | Where-Object { $_ }
    foreach ($p in $paths) {
        $full = Join-Path $clientRoot $p
        if (Test-Path $full) { $scanFiles += $full }
    }
    Write-Host "  Scanning $($scanFiles.Count) specified files"
} else {
    $scanFiles = @(Get-ChildItem -Path $clientRoot -Recurse -Filter "*.razor" -File | ForEach-Object FullName)
    Write-Host "  Scanning all $($scanFiles.Count) Razor files"
}

# Anti-pattern definitions
$antiPatterns = @(
    @{ Name = "@using static ResourceKeys";  Pattern = "@using static\s+.*ResourceKeys";          Tag = "CONVENTION" },
    @{ Name = "L-key string literal";        Pattern = "L\[`"[^`"]";                               Tag = "LOCALIZATION" },
    @{ Name = "Fixed pixel width in Style";  Pattern = "Style\s*=\s*`"[^`"]*width\s*:\s*\d+px";    Tag = "RESPONSIVE" },
    @{ Name = "Fixed MinWidth";              Pattern = "MinWidth\s*=\s*`"\d+px";                  Tag = "RESPONSIVE" }
)

$issueCount = @{}
foreach ($ap in $antiPatterns) { $issueCount[$ap.Name] = 0 }

foreach ($file in $scanFiles) {
    $relativePath = $file.Substring($clientRoot.Length + 1)
    $content = Get-Content -Raw $file -ErrorAction SilentlyContinue
    if (-not $content) { continue }

    foreach ($ap in $antiPatterns) {
        $matches = [regex]::Matches($content, $ap.Pattern, "Multiline")
        if ($matches.Count -gt 0) {
            $issueCount[$ap.Name] += $matches.Count

            if ($ap.Name -eq "L-key string literal") {
                $uniqueKeys = $matches | ForEach-Object { $_.Value } | Select-Object -Unique
                Write-Host "  WARN  $relativePath - $($uniqueKeys.Count) L-key literal(s)" -ForegroundColor Yellow
                if ($uniqueKeys.Count -le 5) {
                    foreach ($k in $uniqueKeys) { Write-Host "        $k" -ForegroundColor DarkYellow }
                }
            } else {
                Write-Host "  WARN  $relativePath - $($ap.Name): $($matches.Count)x" -ForegroundColor Yellow
            }
        }
    }
}

# Summary
Write-Host ""
Write-Host "--- Anti-Pattern Summary ---" -ForegroundColor White
foreach ($ap in $antiPatterns) {
    $count = $issueCount[$ap.Name]
    $color = if ($count -eq 0) { "Green" } else { "Yellow" }
    $icon = if ($count -eq 0) { "PASS" } else { "WARN" }
    Write-Host "  $icon  [$($ap.Tag)] $($ap.Name): $count" -ForegroundColor $color
    if ($count -gt 0) { $warned++ } else { $passed++ }
}

# ------------------------------------------------------------------
# 3. RESX CONSISTENCY CHECK
# ------------------------------------------------------------------
Write-Host ""
Write-Host ">>> STEP 3: Resx Consistency" -ForegroundColor White

$resxEnPath = Join-Path $clientRoot "Resources/Localization.SharedResource.resx"
$resxViPath = Join-Path $clientRoot "Resources/Localization.SharedResource.vi.resx"

if ((Test-Path $resxEnPath) -and (Test-Path $resxViPath)) {
    try {
        $resxEn = [xml](Get-Content $resxEnPath -Raw)
        $resxVi = [xml](Get-Content $resxViPath -Raw)

        $enKeys = @($resxEn.root.data | ForEach-Object name)
        $viKeys = @($resxVi.root.data | ForEach-Object name)

        $enCount = $enKeys.Count
        $viCount = $viKeys.Count

        $onlyEn = $enKeys | Where-Object { $_ -notin $viKeys }
        $onlyVi = $viKeys | Where-Object { $_ -notin $enKeys }

        Write-Host "  EN: $enCount keys  |  VI: $viCount keys" -ForegroundColor White

        if ($onlyEn.Count -eq 0 -and $onlyVi.Count -eq 0) {
            Write-Host "  PASS  EN and VI resx are in sync" -ForegroundColor Green
            $passed++
        } else {
            if ($onlyEn.Count -gt 0) {
                Write-Host "  WARN  $($onlyEn.Count) keys in EN missing in VI" -ForegroundColor Yellow
                $warned++
            }
            if ($onlyVi.Count -gt 0) {
                Write-Host "  WARN  $($onlyVi.Count) keys in VI missing in EN" -ForegroundColor Yellow
                $warned++
            }
        }
    } catch {
        Write-Host "  ERROR  Failed to parse resx: $_" -ForegroundColor Red
        $failed++
    }
} else {
    Write-Host "  SKIP  Resx files not found" -ForegroundColor DarkGray
}

# ------------------------------------------------------------------
# 4. RESULT
# ------------------------------------------------------------------
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
$resultColor = if ($failed -gt 0) { "Red" } elseif ($warned -gt 0) { "Yellow" } else { "Green" }
Write-Host "  RESULT: $passed passed, $warned warnings, $failed failures" -ForegroundColor $resultColor
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

if ($failed -gt 0) { exit 1 } else { exit 0 }
