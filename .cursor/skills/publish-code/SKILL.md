---
name: publish-code
description: "Publish ThaiX.Presentation as a deployment ZIP or WebDeploy target using publish profiles and repeatable script commands. Use when packaging or deploying the application."
argument-hint: "Choose Mode (Package or WebDeploy), project path, configuration, and publish settings profile details"
---

# Publish Skill

Use this skill when you need one of these outcomes for `ThaiX.Presentation`:

- Create a self-contained publish ZIP package for manual deployment.
- Publish directly to hosting with MSDeploy using a `.publishSettings` file.

## What it does

- Runs `dotnet publish` for `src/ThaiX.Presentation/ThaiX.Presentation.csproj`.
- Supports two modes in `.cursor/skills/publish-code/publish.ps1`:
  - `Package` for local artifacts.
  - `WebDeploy` to deploy directly to hosting using the default `docs/thaix.tryasp.net-WebDeploy.publishSettings` profile.
- Defaults to `WebDeploy` for product publishing so the script uses the publish settings file automatically.
- Always cleans the local publish and deploy folders before publishing so each run starts from a fresh output state.

### 1) Package mode

- Produces a `publish` output folder in the repo root.
- Creates/clears a `deploy` folder.
- Writes `deploy/publish.zip` containing the published application.

### 2) WebDeploy mode

- Reads MSDeploy settings from a `.publishSettings` XML file.
- Selects the first `publishProfile` with `publishMethod="MSDeploy"`, or a specific profile by name.
- Publishes directly to remote hosting using MSDeploy properties.
- Avoids printing password values to console output.

## How to use

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .cursor/skills/publish-code/publish.ps1
```

### Optional parameters

Package mode:

```powershell
powershell -ExecutionPolicy Bypass -File .cursor/skills/publish-code/publish.ps1 -Mode Package -Project 'src/ThaiX.Presentation/ThaiX.Presentation.csproj' -Configuration Release -PublishDir publish -DeployDir deploy -ZipName publish.zip
```

WebDeploy mode using publishSettings:

```powershell
powershell -ExecutionPolicy Bypass -File .cursor/skills/publish-code/publish.ps1 -Mode WebDeploy -Project 'src/ThaiX.Presentation/ThaiX.Presentation.csproj' -Configuration Release -PublishSettingsPath 'docs/thaix.tryasp.net-WebDeploy.publishSettings'
```

WebDeploy mode with explicit profile selection:

```powershell
powershell -ExecutionPolicy Bypass -File .cursor/skills/publish-code/publish.ps1 -Mode WebDeploy -PublishSettingsPath 'docs/thaix.tryasp.net-WebDeploy.publishSettings' -PublishProfileName 'site63590-WebDeploy'
```

## Notes

- This mirrors the Visual Studio workflow from MonsterASP docs: import `.publishSettings`, then publish.
- Treat `.publishSettings` as a secret because it can contain credentials.
- For CI/CD, prefer injecting password from environment variable `MONSTERASP_WEBDEPLOY_PASSWORD` instead of storing plaintext secrets in repo.
- In package mode, deploy the full published folder, not only `wwwroot`.
- For IIS targets, keep `web.config` and `ThaiX.Presentation.dll` together.
