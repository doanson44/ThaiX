param(
    [ValidateSet('Package', 'WebDeploy')]
    [string]$Mode = 'WebDeploy',
    [string]$Project = 'src/ThaiX.Presentation/ThaiX.Presentation.csproj',
    [string]$Configuration = 'Release',
    [string]$PublishDir = 'publish',
    [string]$DeployDir = 'deploy',
    [string]$ZipName = 'publish.zip',
    [string]$Runtime = '',
    [bool]$SelfContained = $false,
    [string]$PublishSettingsPath = 'docs/thaix.tryasp.net-WebDeploy.publishSettings',
    [string]$PublishProfileName = ''
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptDir '..\..\..')
Set-Location -Path $repoRoot

$projectPath = Resolve-Path -Path $Project -ErrorAction Stop
$ZipPath = Join-Path $DeployDir $ZipName

Write-Host "Mode: $Mode"
Write-Host "Project: $projectPath"
Write-Host "Configuration: $Configuration"

if (Test-Path $PublishDir) {
    Write-Host "Removing existing publish folder: $PublishDir"
    Remove-Item -LiteralPath $PublishDir -Recurse -Force
}

if (Test-Path $DeployDir) {
    Write-Host "Clearing existing deploy folder: $DeployDir"
    Get-ChildItem -Path $DeployDir -Force | Remove-Item -Recurse -Force
}
else {
    Write-Host "Creating deploy folder: $DeployDir"
    New-Item -ItemType Directory -Path $DeployDir | Out-Null
}

# Restore client libraries (libman) so wwwroot/lib content is included in publish output
Write-Host "Restoring client libraries (libman) for ThaiX.Client..."
Push-Location -Path (Join-Path $repoRoot 'src/ThaiX.Client')
dotnet tool restore
dotnet libman restore
$libmanExitCode = $LASTEXITCODE
Pop-Location
if ($libmanExitCode -ne 0) {
    Write-Error "libman restore failed with exit code $libmanExitCode"
    exit $libmanExitCode
}

if ($Mode -eq 'Package') {
    Write-Host "Publish output: $PublishDir"
    Write-Host "Deploy folder: $DeployDir"
    Write-Host "Zip file: $ZipPath"
    if (-not [string]::IsNullOrWhiteSpace($Runtime)) {
        Write-Host "Runtime: $Runtime"
        Write-Host "Self-contained: $SelfContained"
    }

    $publishArgs = @('publish', $projectPath, '-c', $Configuration, '-o', $PublishDir)
    if (-not [string]::IsNullOrWhiteSpace($Runtime)) {
        $publishArgs += @('--runtime', $Runtime)
        if ($SelfContained) {
            $publishArgs += '--self-contained'
        }
    }

    dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host "Creating zip archive from publish output"
    Compress-Archive -Path (Join-Path $PublishDir '*') -DestinationPath $ZipPath -Force

    Write-Host "Publish and zip complete. Copy $ZipPath to hosting or extract it on the server."
    exit 0
}

$publishSettingsAbsolute = Resolve-Path -Path $PublishSettingsPath -ErrorAction SilentlyContinue
if (-not $publishSettingsAbsolute) {
    Write-Error "Publish settings file not found: $PublishSettingsPath"
    exit 1
}

[xml]$publishSettingsXml = Get-Content -LiteralPath $publishSettingsAbsolute
$profiles = @($publishSettingsXml.publishData.publishProfile | Where-Object { $_.publishMethod -eq 'MSDeploy' })
if ($profiles.Count -eq 0) {
    Write-Error "No MSDeploy publishProfile entries found in $publishSettingsAbsolute"
    exit 1
}

$selectedProfile = $null
if ([string]::IsNullOrWhiteSpace($PublishProfileName)) {
    $selectedProfile = $profiles[0]
}
else {
    $selectedProfile = $profiles | Where-Object { $_.profileName -eq $PublishProfileName } | Select-Object -First 1
}

if (-not $selectedProfile) {
    Write-Error "Could not find profile '$PublishProfileName' in $publishSettingsAbsolute"
    exit 1
}

$publishUrl = $selectedProfile.publishUrl
$msdeploySite = $selectedProfile.msdeploySite
$userName = $selectedProfile.userName
$password = $selectedProfile.userPWD

if ([string]::IsNullOrWhiteSpace($password) -and -not [string]::IsNullOrWhiteSpace($env:MONSTERASP_WEBDEPLOY_PASSWORD)) {
    $password = $env:MONSTERASP_WEBDEPLOY_PASSWORD
}

if ([string]::IsNullOrWhiteSpace($publishUrl) -or [string]::IsNullOrWhiteSpace($msdeploySite) -or [string]::IsNullOrWhiteSpace($userName) -or [string]::IsNullOrWhiteSpace($password)) {
    Write-Error "Publish profile is missing required values (publishUrl, msdeploySite, userName, userPWD)."
    exit 1
}

Write-Host "Using publish settings: $publishSettingsAbsolute"
Write-Host "Publish profile: $($selectedProfile.profileName)"
Write-Host "MSDeploy endpoint: $publishUrl"
Write-Host "MSDeploy site: $msdeploySite"
Write-Host "MSDeploy user: $userName"
Write-Host "Starting WebDeploy publish..."

dotnet msbuild $projectPath /t:Publish /p:Configuration=$Configuration `
    /p:WebPublishMethod=MSDeploy `
    /p:PublishProvider=WebDeploy `
    /p:MSDeployPublishMethod=WMSVC `
    /p:MSDeployServiceURL=$publishUrl `
    /p:PublishUrl=$publishUrl `
    /p:DeployIisAppPath=$msdeploySite `
    /p:UserName=$userName `
    /p:Password=$password `
    /p:AllowUntrustedCertificate=true `
    /p:SkipExtraFilesOnServer=true

if ($LASTEXITCODE -ne 0) {
    Write-Error "WebDeploy publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "WebDeploy publish complete. Verify result at destination URL from the publish profile."
