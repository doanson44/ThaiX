---
name: publish-code
description: "Publish ThaiX.Presentation either as a local ZIP package or directly to hosting via MSDeploy using a .publishSettings profile. Includes a ready-to-run PowerShell script in the same skill folder."
argument-hint: "Choose Mode (Package or WebDeploy), project path, configuration, and publish settings profile details"
---

# Publish Skill

Use this skill when you need one of these outcomes for `ThaiX.Presentation`:

- Create a self-contained publish ZIP package for manual deployment.
- Publish directly to hosting with MSDeploy using a `.publishSettings` file.

## What it does

- Runs `dotnet publish` for `src/ThaiX.Presentation/ThaiX.Presentation.csproj`.
- Supports two modes in `.agents/skills/publish-code/publish.ps1`:
  - `Package` for local artifacts.
  - `WebDeploy` to deploy directly to hosting using an explicitly supplied `.publishSettings` file.
- Defaults to `Package`. Remote deployment must always be explicitly selected with `-Mode WebDeploy`.
- In Package mode, clears only a dedicated `PublishDir` located inside the repository and rejects the repository root or paths outside it. It preserves unrelated files in `DeployDir`; `Compress-Archive -Force` replaces only the target ZIP.

### 1) Package mode

- Produces a `publish` output folder in the repo root.
- Creates the `deploy` folder when needed; it does not empty the folder.
- Writes `deploy/publish.zip` containing the published application.

### 2) WebDeploy mode

- Reads MSDeploy settings from a `.publishSettings` XML file.
- Selects the first `publishProfile` with `publishMethod="MSDeploy"`, or a specific profile by name.
- Publishes directly to remote hosting using MSDeploy properties.
- Avoids printing password values to console output.

## How to use

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .agents/skills/publish-code/publish.ps1
```

### Optional parameters

Package mode:

```powershell
powershell -ExecutionPolicy Bypass -File .agents/skills/publish-code/publish.ps1 -Mode Package -Project 'src/ThaiX.Presentation/ThaiX.Presentation.csproj' -Configuration Release -PublishDir publish -DeployDir deploy -ZipName publish.zip
```

WebDeploy mode using publishSettings:

```powershell
powershell -ExecutionPolicy Bypass -File .agents/skills/publish-code/publish.ps1 -Mode WebDeploy -Project 'src/ThaiX.Presentation/ThaiX.Presentation.csproj' -Configuration Release -PublishSettingsPath '<path-to-secret-publishSettings>'
```

WebDeploy mode with explicit profile selection:

```powershell
powershell -ExecutionPolicy Bypass -File .agents/skills/publish-code/publish.ps1 -Mode WebDeploy -PublishSettingsPath '<path-to-secret-publishSettings>' -PublishProfileName '<profile-name>'
```

## Notes

- This mirrors the Visual Studio workflow from MonsterASP docs: import `.publishSettings`, then publish.
- Treat `.publishSettings` as a secret because it can contain credentials; never commit it.
- The script does not disable TLS certificate validation. Do not add `AllowUntrustedCertificate=true` to bypass an invalid certificate.
- The password is passed to MSBuild through a process-scoped environment variable rather than a command-line property; same-user process inspection and the publish-settings file still require appropriate host security.
- Verify the target host/profile before running WebDeploy. Package mode is the safe default.
- The script prefers `MONSTERASP_WEBDEPLOY_PASSWORD`; it falls back to the password in the publish profile only when the environment variable is empty. Prefer the environment variable and keep publish-settings files out of source control.
- In package mode, deploy the full published folder, not only `wwwroot`.
- For IIS targets, keep `web.config` and `ThaiX.Presentation.dll` together.
