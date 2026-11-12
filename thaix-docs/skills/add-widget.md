# Add Widget Workflow

Follow the current implementation in `.agents/skills/add-widget/SKILL.md` as the primary
reference. The rules below capture its important contracts and guard against older templates.

## Application

- Use `IAppQuery<T>` for read-only queries when following the current Application messaging
  contract.
- Keep private upstream deserialization records in the handler file.
- Map upstream data manually to Application response DTOs; do not use AutoMapper.
- Keep provider transport errors on the existing external-service exception/error-mapping path.
- Do not localize UI messages in Application.
- Distinguish payload-level upstream availability (`Success` / `Message`, when part of the
  feature contract) from the outer `ApiResponse<T>` envelope.

## Infrastructure and endpoint

- Add provider methods to `IExternalDataService` and its Infrastructure implementation.
- Providers use `ExternalApiService.ExecuteAsync<T>()`, configured options, and the shared
  transport/resilience path. Do not add direct HttpClient calls in Application/Presentation or
  hardcode upstream URLs.
- Keep the Minimal API endpoint thin, use MediatR, standard response envelope, correlation ID,
  and the correct `Permissions.*` policy.

## Client

- Use a separate client DTO; never expose server-side types to Blazor WASM.
- Reuse existing `GetEnvelopeAsync` / `GetEnvelopeCachedAsync` helpers and existing cache group
  and TTL constants. Do not add a second envelope parser or manual cache implementation.
- Do not display raw upstream technical error messages directly to users; use localized resources.
- Prefer self-fetching widgets when the widget owns its data. Use a parameter only when the parent
  already owns shared data; do not mix both patterns.
- Log unexpected failures through the injected logger and show a localized error state.

## UI and localization

- Use `IStringLocalizer<SharedResource>` and `ResourceKeys` for every user-facing string.
- Keep English and Vietnamese resource keys synchronized; Vietnamese values require proper
  diacritics.
- Use `IViewportService`, responsive Radzen columns, mobile-safe heading sizes, and dispose
  breakpoint subscriptions.
- Avoid fixed pixel widths. Validate at 375px, 768px, and 1280px.

## Verification

Run relevant Application and Client builds plus focused tests. Report actual command output; do
not claim checks that were not run.
