# ThaiX Design Asset Catalog

Source archive: `THAIX-design.zip`
Source format: 32 SVG artboards
Archive size: approximately 123 MB

## Important handling note

These files are full-page design boards, not one-file-per-icon exports. They contain
embedded images and vector paths. Keep the original archive as the design master; do not
copy all 32 large artboards into the Blazor `wwwroot` directory. Doing so would add more
than 120 MB of mostly reference material to every client deployment.

Use the boards to identify a visual concept, then export only the individual production
asset needed by a specific page. Prefer optimized, transparent SVG for simple marks and
icons, or WebP/PNG for raster artwork. Keep source and optimized output separate.

## System and cross-domain boards

| Source board | Intended coverage |
|---|---|
| ThaiX Icon System | Brand mark and shared icon language |
| System & Navigation | Application shell, navigation, common controls |
| Media & System | Shared media and system concepts |

## Enterprise and business

| Source board | Intended coverage |
|---|---|
| Enterprise & Business | Enterprise modules, teams, workflow |
| Enterprise & Business (2) | Alternate/extended enterprise concepts |

## Finance and markets

| Source board | Intended coverage |
|---|---|
| Finance & Investment | Investment, portfolio, finance |
| Financial Markets | Market data, trading, instruments |
| Finance & Markets | Finance and market concept overlap |

## Blockchain and AI

| Source board | Intended coverage |
|---|---|
| Blockchain & Web3 | Wallets, chains, tokens, Web3 |
| Blockchain & AI | Blockchain and AI concept overlap |
| Artificial Intelligence | AI capabilities and assistants |

## Data and software

| Source board | Intended coverage |
|---|---|
| Data & Analytics | Metrics, dashboards, analytics |
| Software & Development | Developer tools and software concepts |

## Gaming and entertainment

| Source board | Intended coverage |
|---|---|
| Gaming - Core | Game system and shared mechanics |
| Gaming - Game Mechanics I | Core gameplay mechanics |
| Gaming - Game Mechanics II | Extended gameplay mechanics |
| Gaming - Economy & Competition | Resources, progression, competition |
| Gaming & Entertainment | Gaming and entertainment concepts |
| Media & Entertainment | Media content and playback |

## Education, security, commerce

| Source board | Intended coverage |
|---|---|
| Education & Knowledge | Learning and knowledge |
| Security & Administration | Security, access, administration |
| E-commerce & Automation | Commerce and automation |

## ThaiX Tower Defense asset family

| Source board | Intended coverage |
|---|---|
| ThaiX Tower Defense | Game identity and overview |
| Thap va vu khi | Towers and weapons |
| Ho tro va phong thu dac biet | Support and special defense |
| Nang cap va trang thai thap | Upgrades and tower states |
| Cac lop ke dich | Enemy classes |
| Hieu ung chien dau | Combat effects |
| Ban do va chien truong | Maps and battlefields |
| Dot chien va tai nguyen | Waves and resources |
| Can cu va chi huy chien thuat | Base and tactical command |
| Ky nang va HUD chien dau | Skills and combat HUD |

## Icon usage map for the current application

The mapping below is semantic guidance. Existing route names, permission checks, and
localized labels remain authoritative.

| ThaiX module | Existing UI concept | Icon intent |
|---|---|---|
| Home | Home | Home/dashboard |
| Market Data | Market data | Trending chart / exchange |
| Market Research | Research | Analytics / lab |
| ChainBroker | Blockchain data | Network / hub |
| Portfolio | Portfolio | Wallet / pie chart |
| CRM | Contacts and notes | People / contacts |
| Master Data | Countries, cities, banks | Database / folder |
| Notifications | Sender and schedules | Bell / schedule |
| Security | API clients and credentials | Shield / key |
| Administration | Users and system tools | Admin panel / settings |
| Games | Game catalog | Game controller |
| Blog and Media | Articles and content | Article / media |

## Production asset checklist

Before adding any extracted asset to `src/ThaiX.Client/wwwroot`:

- Confirm its intended module and semantic name.
- Confirm that it is an individual asset, not a whole reference board.
- Confirm transparent background and viewBox/canvas bounds.
- Remove unused metadata and embedded source payloads where possible.
- Optimize file size without damaging visual quality.
- Use a stable kebab-case filename and document the source board.
- Verify rendering in both themes and at mobile/tablet/desktop sizes.
- Do not replace an existing icon globally until every usage has been checked.

## Extracted runtime icon set

The first runtime extraction is available at `src/ThaiX.Client/wwwroot/icons/system.svg`.
It contains 14 monochrome, editable vector glyphs recreated from the named visual
references in the `System & Navigation` board and normalized to a 24 x 24 viewBox:

| Name | Meaning |
|---|---|
| `home` | Home |
| `dashboard` | Dashboard |
| `applications` | Applications |
| `workspace` | Workspace |
| `calendar` | Calendar |
| `clock` | Clock |
| `history` | History |
| `bookmark` | Bookmark |
| `favorites` | Favorites |
| `help` | Help |
| `profile` | Profile |
| `language` | Language |
| `theme` | Theme |
| `fullscreen` | Fullscreen |

Use the shared Blazor component:

```razor
<ThaiXIcon Name="home" Size="1.25rem" Title="Home" />
<ThaiXIcon Name="dashboard" Class="text-primary" />
```

The component renders an external SVG symbol with `currentColor`, so it inherits the
surrounding text color and remains theme-aware. Provide `Title` when the icon conveys
meaning by itself; omit it when the icon is decorative and the surrounding control has
its own accessible name.

The archive also contains 1,214 additional embedded image occurrences across domain,
media, and Tower Defense boards. Those are not silently treated as vector icons: the
source artboards embed PNG images, not editable vector paths, for those assets. They
require a separate asset pass to classify semantic names, decide which belong in the
UI icon set versus game illustrations, and retain their raster fidelity or explicitly
trace them to vectors. The current runtime extraction is therefore the reusable system
icon foundation, not the complete 32-board asset conversion.
