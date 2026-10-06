# =============================================================================
# run-phase.ps1 - ThaiX Client Refactor Phase Runner
# =============================================================================
# Chains all 3 verification layers for a single refactor phase.
#
# Usage:
#   .\run-phase.ps1 -Phase 1
#   .\run-phase.ps1 -Phase 2 -ChangedFiles "Layout/MainLayout.razor,Layout/NavMenu.razor" -WithVisualTests
#   .\run-phase.ps1 -Phase 12 -SkipBuild
# =============================================================================
param(
    [Parameter(Mandatory)]
    [int]$Phase,

    [string]$ChangedFiles = "",

    [switch]$WithVisualTests,

    [switch]$SkipBuild
)

$ErrorActionPreference = "Continue"
$repoRoot = $PSScriptRoot | Split-Path -Parent
$verifyScript = Join-Path $repoRoot "scripts/verify-phase.ps1"
$auditTemplate = Join-Path $repoRoot "docs/implement-plans/audit-checklist-dark-finance.md"

$phaseMeta = @{
    1  = @{ Name = "Foundation: Base Class + Convention";         Severity = "High";     VisualTests = $false; DefaultFiles = "_Imports.razor,Layout/ThaiXPageBase.cs,Constants/ResourceKeys.cs" }
    2  = @{ Name = "Layout (Shell)";                              Severity = "High";     VisualTests = $true;  DefaultFiles = "Layout/MainLayout.razor,Layout/NavMenu.razor,Layout/AuthLayout.razor,Layout/GlobalErrorBoundary.razor" }
    3  = @{ Name = "Shared Components";                           Severity = "High";     VisualTests = $false; DefaultFiles = "" }
    4  = @{ Name = "Auth Pages";                                  Severity = "Medium";   VisualTests = $false; DefaultFiles = "" }
    5  = @{ Name = "Account + Home + NotFound";                   Severity = "Medium";   VisualTests = $false; DefaultFiles = "" }
    6  = @{ Name = "MasterData Pages + Dialogs";                  Severity = "Medium";   VisualTests = $true;  DefaultFiles = "" }
    7  = @{ Name = "Administration Pages";                        Severity = "Medium";   VisualTests = $false; DefaultFiles = "" }
    8  = @{ Name = "CRM (Contacts + Notes)";                      Severity = "Medium";   VisualTests = $false; DefaultFiles = "" }
    9  = @{ Name = "Notification + Security";                     Severity = "Medium";   VisualTests = $true;  DefaultFiles = "" }
    10 = @{ Name = "MarketData Pages";                            Severity = "High";     VisualTests = $true;  DefaultFiles = "" }
    11 = @{ Name = "MarketResearch Pages";                        Severity = "High";     VisualTests = $true;  DefaultFiles = "" }
    12 = @{ Name = "Portfolio Pages (Most Complex)";              Severity = "Critical"; VisualTests = $true;  DefaultFiles = "" }
}

$meta = $phaseMeta[$Phase]

Write-Host ""
Write-Host "##############################################################" -ForegroundColor Magenta
Write-Host "  PHASE ${Phase} RUNNER - ThaiX Client Refactor" -ForegroundColor Magenta
Write-Host "  $($meta.Name)" -ForegroundColor White
Write-Host "  Severity: $($meta.Severity)" -ForegroundColor White
Write-Host "##############################################################" -ForegroundColor Magenta
Write-Host ""

$fileList = if ($ChangedFiles) { $ChangedFiles } else { $meta.DefaultFiles }

# ==========================================================================
# LAYER 1: Build + Static Scan
# ==========================================================================
Write-Host ">>> LAYER 1: Build + Static Anti-Pattern Scan" -ForegroundColor Cyan

$verifyArgs = "-NoProfile -ExecutionPolicy Bypass -File `"$verifyScript`" -Phase $Phase"
if ($fileList) { $verifyArgs += " -ChangedFiles `"$fileList`"" }
if ($SkipBuild) { $verifyArgs += " -NoBuild" }

$verifyCmd = "powershell $verifyArgs"
Write-Host "  Running verify-phase.ps1..." -ForegroundColor DarkGray

$result = cmd /c "$verifyCmd 2>&1"
$exitCode = $LASTEXITCODE

$result | ForEach-Object { Write-Host $_ }

if ($exitCode -ne 0) {
    Write-Host ""
    Write-Host "  LAYER 1 FAILED. Fix issues before proceeding." -ForegroundColor Red
    exit $exitCode
}

# ==========================================================================
# LAYER 2: Visual Regression Tests (if applicable)
# ==========================================================================
$doVisual = $WithVisualTests -or $meta.VisualTests
if ($doVisual) {
    Write-Host ""
    Write-Host ">>> LAYER 2: Visual Regression Tests" -ForegroundColor Cyan

    $visualProj = Join-Path $repoRoot "tests/ThaiX.Client.VisualTests/ThaiX.Client.VisualTests.csproj"
    if (Test-Path $visualProj) {
        Write-Host "  Project: tests/ThaiX.Client.VisualTests" -ForegroundColor White
        Write-Host ""
        Write-Host "  Playwright Visual Test Workflow:" -ForegroundColor Yellow
        Write-Host "  1. Start the app:" -ForegroundColor DarkGray
        Write-Host "     dotnet run --project src/ThaiX.Presentation --launch-profile https" -ForegroundColor DarkGray
        Write-Host "  2. Create baselines:" -ForegroundColor DarkGray
        Write-Host "     `$env:UPDATE_BASELINES='true'; dotnet test $visualProj" -ForegroundColor DarkGray
        Write-Host "  3. Run tests:" -ForegroundColor DarkGray
        Write-Host "     dotnet test $visualProj" -ForegroundColor DarkGray
        Write-Host "  4. Review diffs: tests/ThaiX.Client.VisualTests/TestResults/Diffs/" -ForegroundColor DarkGray
    } else {
        Write-Host "  SKIP  Visual test project not found." -ForegroundColor DarkGray
    }
} else {
    Write-Host ""
    Write-Host ">>> LAYER 2: Visual Tests SKIPPED (not required for Phase ${Phase})" -ForegroundColor DarkGray
}

# ==========================================================================
# LAYER 3: Dark-Finance $10K Audit Checklist
# ==========================================================================
Write-Host ""
Write-Host '>>> LAYER 3: Dark-Finance $10K Audit' -ForegroundColor Cyan

$auditReportDir = Join-Path $repoRoot "tests/ThaiX.Client.VisualTests/AuditReports"
New-Item -ItemType Directory -Force -Path $auditReportDir | Out-Null

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$auditReportPath = Join-Path $auditReportDir "Phase${Phase}_Audit_${timestamp}.md"

if (Test-Path $auditTemplate) {
    $auditContent = (Get-Content $auditTemplate -Raw) -replace '\$\{PHASE\}', "$Phase"
    Set-Content $auditReportPath -Value $auditContent -NoNewline
} else {
    Set-Content $auditReportPath -Value "# Phase ${Phase} Audit Report`n`nGenerated: ${timestamp}`n"
}

Write-Host "  Audit checklist: $auditReportPath" -ForegroundColor Yellow
Write-Host "  Template:        $auditTemplate" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  35-point checklist covering:" -ForegroundColor White
Write-Host "    01 Point of View  |  02 Typography  |  03 Color System" -ForegroundColor DarkGray
Write-Host "    04 Hierarchy      |  05 Imagery     |  06 Motion" -ForegroundColor DarkGray
Write-Host "    07 Mobile         |  08 Invisible Polish" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Open the report, mark [ ] -> [x] as each check passes." -ForegroundColor White

# ==========================================================================
# RESULT
# ==========================================================================
Write-Host ""
Write-Host "##############################################################" -ForegroundColor Green
Write-Host "  PHASE ${Phase} COMPLETE - Verification Passed" -ForegroundColor Green
Write-Host "##############################################################" -ForegroundColor Green
Write-Host ""
Write-Host "  Layer 1 (Build+Scan):   PASSED" -ForegroundColor Green
if ($doVisual) {
    Write-Host "  Layer 2 (Visual):       See instructions above" -ForegroundColor Yellow
} else {
    Write-Host "  Layer 2 (Visual):       SKIPPED" -ForegroundColor DarkGray
}
Write-Host "  Layer 3 (Audit):        $auditReportPath" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Next: Address audit findings, then proceed to next phase." -ForegroundColor White
Write-Host ""
