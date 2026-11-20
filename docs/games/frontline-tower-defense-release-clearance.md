# Frontline Tower Defense Release Clearance

## Decision

**Release status: BLOCKED - original game bundle and artwork distribution rights are not verified.**

The legacy JavaScript runtime and its original asset directory remain in the repository as requested for behavioral comparison. Their presence in the repository must not be treated as permission to redistribute them in a public or commercial release.

## Repository evidence

- The repository root contains an Apache License 2.0 file.
- The game asset directory contains 2,193 bitmap files and 20 audio files.
- No game-specific asset license, copyright notice, attribution manifest, or root NOTICE file was found for this bundle.
- The bundled JavaScript contains the legacy save namespace `FD-2-32-317`, but the inspected file does not identify a copyright owner or provide a redistribution grant.
- The Apache license grants rights from contributors for work they are authorized to license. The repository license alone does not establish that the repository contributors own or can sublicense the original game code, bitmap art, or sound files.

## Scope affected

| Material | Current state | Release decision |
|---|---|---|
| Legacy JavaScript runtime | Retained as behavioral reference | Do not redistribute until cleared |
| Original bitmap artwork | 2,193 files present | Do not redistribute until cleared |
| Original audio | 20 files present | Do not redistribute until cleared |
| New Razor/SVG/CSS artwork | Authored in the C# port; does not reference original bitmap files | Keep separate from legacy asset licensing review |
| C# gameplay implementation | New port code | Review under repository contribution/license policy |

## Required evidence before release

1. Identify the original game copyright owner and the owner or licensor of the bitmap and audio assets.
2. Obtain written permission or a license that explicitly covers redistribution of the game bundle, source/runtime code, original artwork, audio, and derivative/ported implementations.
3. Confirm whether commercial distribution, hosting, modification, and sublicensing are permitted, and record required attribution and notices.
4. Add a per-asset or bundle-level attribution/license manifest and retain the permission documents in the project's legal records.
5. Re-run this review against the exact release artifact and confirm that every included legacy file is covered.

## Engineering safeguard

Do not delete or rewrite the legacy runtime during the port. Keep it available for local behavioral comparison. Before any release build is approved, decide whether the legacy runtime and original assets are legally cleared for distribution or must be excluded from the published artifact while remaining in the source repository.

This is an engineering clearance record, not legal advice.
