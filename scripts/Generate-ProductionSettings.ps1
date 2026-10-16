param(
    [string]$DevelopmentPath,
    [string]$TemplatePath,
    [string]$OutputPath,
    [switch]$AllowUnresolved,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$presentationPath = Join-Path $repoRoot "src/ThaiX.Presentation"

if ([string]::IsNullOrWhiteSpace($DevelopmentPath)) {
    $DevelopmentPath = Join-Path $presentationPath "appsettings.Development.json"
}
if ([string]::IsNullOrWhiteSpace($TemplatePath)) {
    $TemplatePath = Join-Path $presentationPath "appsettings.json"
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $presentationPath "appsettings.Production.json"
}

foreach ($path in @($DevelopmentPath, $TemplatePath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required input file was not found: $path"
    }
}

if ((Test-Path -LiteralPath $OutputPath -PathType Leaf) -and -not $Force) {
    throw "Output already exists: $OutputPath. Review it first or rerun with -Force to overwrite it."
}

function Test-DummyValue {
    param([object]$Value)

    if ($Value -isnot [string]) {
        return $false
    }

    return $Value -match '(?i)REPLACE_WITH|example\.invalid|\.invalid(?:[:/]|$)'
}

function Merge-DummyValues {
    param(
        [object]$Target,
        [object]$Source,
        [string]$Path
    )

    if ($Target -is [System.Collections.IDictionary] -and
        $Source -is [System.Collections.IDictionary]) {
        foreach ($key in @($Target.Keys)) {
            if ($Source.Contains($key)) {
                $Target[$key] = Merge-DummyValues -Target $Target[$key] -Source $Source[$key] -Path "$Path.$key"
            }
        }
        return ,$Target
    }

    if ($Target -is [System.Collections.IList] -and
        $Target -isnot [string] -and
        $Source -is [System.Collections.IList] -and
        $Source -isnot [string]) {
        for ($index = 0; $index -lt $Target.Count -and $index -lt $Source.Count; $index++) {
            $Target[$index] = Merge-DummyValues -Target $Target[$index] -Source $Source[$index] -Path "$Path[$index]"
        }
        return ,$Target
    }

    if ((Test-DummyValue -Value $Target) -and
        $Source -is [string] -and
        -not [string]::IsNullOrWhiteSpace($Source) -and
        -not (Test-DummyValue -Value $Source)) {
        Write-Host "Replacing dummy value: $Path"
        return $Source
    }

    return $Target
}

function Get-DummyPaths {
    param(
        [object]$Node,
        [string]$Path = '$'
    )

    $found = [System.Collections.Generic.List[string]]::new()

    if ($Node -is [System.Collections.IDictionary]) {
        foreach ($key in $Node.Keys) {
            foreach ($item in (Get-DummyPaths -Node $Node[$key] -Path "$Path.$key")) {
                $found.Add($item)
            }
        }
    }
    elseif ($Node -is [System.Collections.IList] -and $Node -isnot [string]) {
        for ($index = 0; $index -lt $Node.Count; $index++) {
            foreach ($item in (Get-DummyPaths -Node $Node[$index] -Path "$Path[$index]")) {
                $found.Add($item)
            }
        }
    }
    elseif (Test-DummyValue -Value $Node) {
        $found.Add($Path)
    }

    return $found
}

$development = Get-Content -LiteralPath $DevelopmentPath -Raw | ConvertFrom-Json -AsHashtable
$template = Get-Content -LiteralPath $TemplatePath -Raw | ConvertFrom-Json -AsHashtable

$production = Merge-DummyValues -Target $template -Source $development -Path '$'
$unresolved = @(Get-DummyPaths -Node $production)

if ($unresolved.Count -gt 0) {
    Write-Warning "Dummy values remain in the generated configuration:"
    $unresolved | ForEach-Object { Write-Warning "  $_" }

    if (-not $AllowUnresolved) {
        throw "No output was written. Add the missing values to appsettings.Development.json, or pass -AllowUnresolved to write a partial file."
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory -PathType Container)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$json = ConvertTo-Json -InputObject $production -Depth 100
Set-Content -LiteralPath $OutputPath -Value $json -Encoding utf8

Write-Host "Production configuration written to: $OutputPath"
Write-Host "Unresolved dummy values: $($unresolved.Count)"
