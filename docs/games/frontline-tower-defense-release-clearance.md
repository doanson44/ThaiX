# Frontline Tower Defense Release Clearance

## Decision

**Release status: BLOCKED - original game bundle and artwork distribution rights are not verified.**

The original asset directory remains in the repository for the C# Flump renderer. The three legacy JavaScript runtime bundles have been removed from this port branch (their source remains recoverable from the parent branch and Git history). Neither the original assets nor historical source presence should be treated as permission to redistribute them in a public or commercial release.

## Repository evidence

- The repository root contains an Apache License 2.0 file.
- The game asset directory contains 2,193 bitmap files and 20 audio files.
- No game-specific asset license, copyright notice, attribution manifest, or root NOTICE file was found for this bundle.
- The bundled JavaScript contains the legacy save namespace `FD-2-32-317`, but the inspected file does not identify a copyright owner or provide a redistribution grant.
- The Apache license grants rights from contributors for work they are authorized to license. The repository license alone does not establish that the repository contributors own or can sublicense the original game code, bitmap art, or sound files.

## External provenance and hosting-policy check

- The [Max Games Frontline Defense 2 page](https://www.maxgames.com/play/frontline-defense-2.html) identifies the original title and provides a Download SWF link.
- The [Max Games free-games page](https://www.maxgames.com/free-games.html) says its game packages include the game file, images, and game information, and invites website hosting with a backlink to Max Games.
- The [Armor Games listing](https://armorgames.com/play/10261/frontline-defense-2) also attributes Frontline Defense 2 to maxgames.
- The hosting page is useful evidence for a limited, linked hosting arrangement for Max Games game packages. It does not expressly address decompilation, a C# reimplementation, derivative works, separate extraction/reuse of artwork or audio, sublicensing, or commercial redistribution of a modified bundle. Those rights remain unverified for this port.

## Scope affected

| Material | Current state | Release decision |
|---|---|---|
| Legacy JavaScript runtime | Removed from this branch; recoverable from parent branch/history | Not part of this branch's runtime; do not redistribute |
| Original bitmap artwork | 2,193 files present | Do not redistribute until cleared |
| Original audio | 20 files present | Do not redistribute until cleared |
| New Razor/SVG/CSS artwork | Authored in the C# port; does not reference original bitmap files | Keep separate from legacy asset licensing review |
| C# gameplay implementation | New port code | Review under repository contribution/license policy |

## Build safeguard

The client project now marks `wwwroot/games/frontline-tower-defense/assets/**/*` with `CopyToPublishDirectory="Never"`. This keeps the legacy bundle in source for behavioral comparison while preventing it from being copied into a publish artifact by the standard MSBuild publish flow. The setting still requires verification with an actual `dotnet publish` output inspection on the project's .NET 11 toolchain. It does not remove the original files from the Git repository or establish rights for repository distribution.

## Required evidence before release

1. Identify the original game copyright owner and the owner or licensor of the bitmap and audio assets.
2. Obtain written permission or a license that explicitly covers redistribution of the game bundle, source/runtime code, original artwork, audio, and derivative/ported implementations.
3. Confirm whether commercial distribution, hosting, modification, and sublicensing are permitted, and record required attribution and notices.
4. Add a per-asset or bundle-level attribution/license manifest and retain the permission documents in the project's legal records.
5. Re-run this review against the exact release artifact and confirm that every included legacy file is covered.

## Engineering safeguard

Do not delete or rewrite the legacy runtime during the port. Keep it available for local behavioral comparison. Before any release build is approved, decide whether the legacy runtime and original assets are legally cleared for distribution or must be excluded from the published artifact while remaining in the source repository.

This is an engineering clearance record, not legal advice.
