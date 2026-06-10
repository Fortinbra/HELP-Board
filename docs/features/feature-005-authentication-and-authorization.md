# Feature 005: Authentication and Authorization

## Goal
Provide secure, organization-aligned authentication and authorization so HELP-Board users can sign in with Google Workspace by default, with Authentik as a fallback OpenID Connect provider.

## User Value
Staff and volunteers can access HELP-Board using familiar accounts while administrators keep role-based access control and security policies consistent.

## Scope
### In Scope
- Implement OIDC/OAuth login with Google Workspace as the primary identity provider.
- Implement Authentik OIDC as a fallback provider when Google integration is unavailable or not approved.
- Define role and permission model for application access control.
- Define user provisioning and deprovisioning approach.
- Define session management and authentication security requirements.
- Define provider failover and operational fallback behavior.

### Out of Scope
- Building a custom username/password identity store.
- Full fine-grained authorization for every future feature module.

## UX
### User Flows
- User selects sign-in and is redirected to Google Workspace login.
- User returns with claims, account is provisioned or updated, and user is routed to dashboard.
- If Google SSO is disabled/unavailable, user is routed to Authentik login path.
- Admin reviews user roles and access assignments.

### Key Screens / Components
- Login entry screen with provider selection and policy notice.
- Access denied page with support guidance.
- Admin role/permission management screen.
- User profile and session status indicators.

### States
- Auth loading/redirect in progress.
- Successful login and new-user provisioning complete.
- Access denied due to missing role.
- Provider unavailable with fallback route available.

## Data Needs
### Data Sources
- OIDC claims from Google Workspace.
- OIDC claims from Authentik fallback provider.
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
- Access denied and provider-fallback states must not rely on color alone.

## Security and Session Requirements
- Validate issuer, audience, signature, expiry, and nonce for all OIDC tokens.
- Enforce HTTPS-only auth callbacks and secure cookie settings.
- Define session timeout and idle timeout values by environment.
- Support forced sign-out/re-authentication for revoked users.
- Record authentication events for auditing and incident review.

## Provider Strategy
- Primary provider: Google Workspace (OIDC/OAuth).
- Fallback provider: Authentik (OIDC).
- Provider routing should be configuration-driven and environment-specific.
- If primary provider health checks fail, fallback can be enabled through operational configuration.

## Dependencies
- Infrastructure support for OAuth/OIDC redirect URIs and secrets management.
- Administrative policy decisions for default roles and least-privilege access.
- Feature 006 CI/CD and Release Deployment for protected config rollout.

## Risks
- Incorrect claim mapping can grant incorrect role access.
- Provider outages can block user access without tested fallback paths.
- Session policy misconfiguration can weaken security posture.

## Acceptance Criteria
- [ ] Google Workspace OIDC login path is documented as the primary SSO method.
- [ ] Authentik OIDC fallback path is documented with activation conditions.
- [ ] Role and permission model is documented with default role assignments.
- [ ] User provisioning/deprovisioning lifecycle is documented.
- [ ] Session and token validation security requirements are documented.
- [ ] Failure/fallback behavior is documented for provider outages and auth errors.

## Open Questions
- Should provider selection be user-facing or fully controlled by configuration?
- What is the minimum role set required for initial production rollout?
- Which claims are authoritative for role bootstrap and org membership?
