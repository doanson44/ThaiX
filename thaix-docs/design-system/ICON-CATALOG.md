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

## Runtime icon library

The production sprite is `src/ThaiX.Client/wwwroot/icons/system-design.svg`. It contains
28 glyphs traced from the 28 white system glyph assets embedded in the `ThaiX Icon System`
artboard, normalized to a 24 x 24 viewBox and converted to monochrome vector paths using
`currentColor`. Three additional utility glyphs (`menu`, `arrow-left`, `logout`) support
the application shell.

| Name | Meaning |
|---|---|
| `home` | Home |
| `analytics` | Analytics and metrics |
| `team` | People and teams |
| `wallet` | Wallet and finance |
| `database` | Data storage |
| `document` | Documents and articles |
| `shield-check` | Security and verification |
| `ai-chip` | Artificial intelligence |
| `cube` | Products and assets |
| `link` | Links and integrations |
| `gamepad` | Games |
| `video` | Video and media |
| `image` | Images |
| `settings` | Settings |
| `globe` | Global and language |
| `leaf` | Sustainability |
| `lightning` | Automation and speed |
| `cart` | Commerce |
| `bell` | Notifications |
| `alert` | Warning and attention |
| `lock` | Access control |
| `cloud` | Cloud |
| `server` | Infrastructure |
| `search` | Search |
| `chat` | Communication |
| `heart` | Favorites |
| `user` | User profile |
| `share` | Sharing |
| `menu` | Navigation menu |
| `arrow-left` | Back and close navigation |
| `logout` | Sign out |

Use the shared component:

```razor
<ThaiXIcon Name="home" Size="1.25rem" Title="Home" />
<ThaiXIcon Name="analytics" Class="text-primary" />
```

The component accepts the canonical glyph names and a compatibility map for common legacy
Radzen/Material icon names. Unknown legacy names resolve to the generic `cube` glyph so
the page does not fall back to a second icon family. A meaningful standalone icon should
receive `Title`; decorative icons inside a labelled control remain hidden from assistive
technology.

The application navigation and profile menu now use the ThaiX glyphs. The shared
`ResponsiveIconButton` also renders the ThaiX set, covering actions across pages that use
that shared component. Remaining direct Radzen controls are being migrated incrementally;
Radzen remains available for non-icon component behavior.

The original artboards contain 1,228 embedded image elements, many of which are branded
variants, domain illustrations, or Tower Defense sprites. They are not all interchangeable
UI glyphs. Keep those source assets classified separately rather than adding every raster
illustration to the system icon sprite.
