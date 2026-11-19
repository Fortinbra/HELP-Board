# Feature 005: Authentication and Authorization

## Goal
Provide secure, organization-aligned authentication and authorization so HELP-Board users can sign in with Google Workspace.

## Planning Note
The [global application requirements](../global-requirements.md) confirm Google Workspace sign-in. Google Workspace sign-in is implemented with one configured domain, Viewer by default, and a configured Admin email list. Authentik fallback is not required at this stage and may be reconsidered later. This initial role mapping is configuration-backed and does not yet provide persistent user provisioning, administrative role management, or immediate revocation.

## User Value
Staff and volunteers can access HELP-Board using familiar accounts while administrators keep role-based access control and security policies consistent.

## Scope
### In Scope
- Implement OIDC/OAuth login with Google Workspace as the primary identity provider.
- Define role and permission model for application access control.
- Define user provisioning and deprovisioning approach.
- Define session management and authentication security requirements.
- Document operational behavior when Google sign-in is unavailable.

### Out of Scope
- Building a custom username/password identity store.
- Full fine-grained authorization for every future feature module.
- Authentik OIDC fallback at this stage; reconsider it separately if needed.

## UX
### User Flows
- User selects sign-in and is redirected to Google Workspace login.
- User returns with claims, account is provisioned or updated, and user is routed to dashboard.
- If Google SSO is disabled or unavailable, the user sees an appropriate sign-in error or unavailable state; no fallback provider is currently implemented.
- Admin reviews user roles and access assignments.

### Key Screens / Components
- Login entry screen with a Google sign-in action and policy notice.
- Access denied page with support guidance.
- Admin role/permission management screen.
- User profile and session status indicators.

### States
- Auth loading/redirect in progress.
- Successful login and new-user provisioning complete.
- Access denied due to missing role.
- Provider unavailable, with no fallback route currently available.

## Data Needs
### Data Sources
- OIDC claims from Google Workspace.
- Internal role and permission assignments.

### Data Model Considerations
- User identity table should store provider subject ID, email, display name, provider source, and last login.
- Authorization model should map users to roles and roles to permissions.
- Audit fields should capture provisioning source and role changes.

### Data Freshness
- Claims resolved per login session.
- Role changes effective on next token validation cycle or sign-in refresh.

## Accessibility (WCAG)
- Sign-in actions must be fully keyboard accessible.
- Login and error messaging must use semantic structure and clear announcements.
- Access denied and provider-unavailable states must not rely on color alone.

## Security and Session Requirements
- Validate issuer, audience, signature, expiry, and nonce for all OIDC tokens.
- Enforce HTTPS-only auth callbacks and secure cookie settings.
- Define session timeout and idle timeout values by environment.
- Support forced sign-out/re-authentication for revoked users.
- Record authentication events for auditing and incident review.

## Provider Strategy
- Current provider: Google Workspace (OIDC/OAuth).
- Authentik fallback is not required at this stage. Any future provider requires a separately approved activation, identity-mapping, and recovery design.

## Dependencies
- Infrastructure support for OAuth/OIDC redirect URIs and secrets management.
- Administrative policy decisions for default roles and least-privilege access.
- Feature 006 CI/CD and Release Deployment for protected config rollout.
- Google integration research outcomes and product decisions on organizational eligibility, session architecture, and rollout timing.

## Risks
- Incorrect claim mapping can grant incorrect role access.
- Google outages can block new sign-ins while there is no fallback provider.
- Session policy misconfiguration can weaken security posture.
- Unresolved eligibility, identity mapping, and session policies can create rework or inconsistent access if implementation proceeds before those decisions are finalized.

## Copilot Governance
### Candidate Instructions
- Require every auth change to document claim mapping, least-privilege expectations, and provider-unavailability behavior before implementation.
- Require authentication work to include secure-by-default handling for token validation, cookies, redirect URIs, and audit events.
- Require user provisioning and deprovisioning changes to call out role bootstrap rules and revocation behavior explicitly.

### Candidate Skills
- Authentication review skill to verify OIDC/OAuth configuration, claim mapping, session controls, and provider-unavailability handling.
- Authorization governance skill to review RBAC changes, provisioning flows, and auditability requirements.

### Reusable Prompts
- "Review this authentication change for least-privilege alignment, provider-unavailability handling, and token/session validation completeness."
- "List the tests and operational checks required when claim mapping, role bootstrap, or provider-unavailability behavior changes."
- "Identify whether this auth work needs new Copilot instructions for security, provisioning, or configuration governance."

## Acceptance Criteria
- [x] Google Workspace OIDC login path is implemented as the primary SSO method.
- [x] Initial role mapping is implemented: Workspace members receive Viewer; configured administrator emails receive Admin.
- [x] The OIDC callback validates the verified Workspace domain and email claim.
- [x] API endpoints require authentication by default; sign-out uses antiforgery protection.
- [x] The public welcome page is anonymous; the dashboard route redirects anonymous users to sign-in when Google is configured.
- [x] The dashboard component verifies session status before loading dashboard content.
- [x] Cookie sessions use HTTPS-only, HttpOnly cookies and an eight-hour lifetime.
- [ ] User provisioning/deprovisioning lifecycle is documented.
- [ ] Immediate session revocation and database-backed user provisioning are not implemented.
- [x] Session and token validation security requirements are documented.
- [ ] Failure behavior is documented for provider outages and auth errors.
- [ ] Copilot governance artifacts are defined for auth work, covering security guardrails, RBAC consistency, and provider-unavailability handling.

## Open Questions
- Google outage handling remains open; a fallback provider is not required at this stage.
- Persistent role management, provisioning/deprovisioning, and immediate session revocation remain open.
