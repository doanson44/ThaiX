# Security and Authentication Rules

## Authentication and validation

- JWT Bearer is the API authentication scheme; the external-login cookie is used for the Google
  callback flow, not as the API's primary authentication scheme.
- Current JWT validation checks issuer, audience, lifetime, and signing key; clock skew is zero.
- Require a valid `actor_type` value: `user` or `system`.
- User tokens require `sub` and `email`; system tokens must not contain an email claim.
- Do not document fixed token TTLs without inspecting the token issuance service and configuration.

## Permission claims

- Permission identifiers are centralized in `ThaiX.Domain.Common.Constants.Permissions`.
- Endpoints must use `.RequireAuthorization(Permissions.X)`; never hardcode permission strings.
- JWT validation copies each non-empty `scope` value to a `permission` claim if that permission
  claim is absent.
- The client currently checks `scope` claims for navigation and client policy registration. Keep
  the `scope` and `permission` contracts distinct; trace issuance, validation, authorization
  handlers, client registration, and tests before changing them.
- Client navigation guards are not a server security boundary.
- Roles are administrative grouping, not business authorization logic.

## Identity boundary

- ASP.NET Core Identity belongs in Infrastructure.
- `ApplicationUser : IdentityUser<Guid>`; do not merge Identity and Domain models.
- Do not put authentication/authorization logic in Domain.

## Logging

Never log secrets, access tokens, passwords, or unnecessary PII. Preserve stable machine-readable
error codes and correlation context.
