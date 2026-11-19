# Feature Backlog

This backlog preserves deferred portions of features for future consideration. It is separate from [on-hold features](README.md#on-hold), which retain their own feature documents. Entries are not current release commitments; review their scope and priority before implementation. Backlogging work does not waive applicable global requirements or release gates, including security and accessibility.

## Authentication

### Reconsider Authentik fallback

- **Origin:** Feature 005: Authentication and Authorization.
- **Deferred:** Authentik OIDC fallback is not required at this stage; Google Workspace remains the current sign-in provider.
- **If taken up:** Define provider activation, account eligibility, identity mapping, outage and recovery procedures, and tests. Preserve the same authorization policy; do not link accounts solely by matching email addresses.
- **Related requirements:** [SEC-002 and SEC-007](../global-requirements.md#8-authentication-authorization-and-privacy).

### Provisioning and access management

- **Origin:** Feature 005: Authentication and Authorization.
- Define eligibility, provisioning and deprovisioning, administrator bootstrap, role changes, and revocation. Document how changes affect existing sessions.
- Implement database-backed user provisioning and persistent role management, including the planned administrator access-management UI. Record provider/subject, email, display name, last login, provisioning source, and auditable role and permission changes.
- Define and verify session and idle timeouts and forced sign-out or reauthentication for revoked users. The current configuration-backed roles use an eight-hour cookie; immediate revocation is not implemented.
- Review fine-grained permissions as future modules are delivered.
- **Related requirements:** [SEC-004, SEC-008, SEC-009, SEC-010, SEC-011, and SEC-013](../global-requirements.md#8-authentication-authorization-and-privacy).

### Google sign-in failures and outages

- **Origin:** Feature 005: Authentication and Authorization.
- Document and verify handling of sign-in errors and Google outages, including accessible unavailable and access-denied states and support guidance. No fallback provider is required at this stage.
- **Related requirements:** [SEC-009, SEC-010, and SEC-013](../global-requirements.md#8-authentication-authorization-and-privacy).

### Authentication governance

- **Origin:** Feature 005: Authentication and Authorization.
- Define appropriate governance artifacts for claim mapping, least privilege and role consistency, token and cookie security, provider-unavailability handling, provisioning, revocation, and authentication audit events. Review whether reusable instructions, prompts, or skills are warranted.
- **Related requirements:** [SEC-003 through SEC-010](../global-requirements.md#8-authentication-authorization-and-privacy).

## Feature Toggles

### Flag inventory and lifecycle

- **Origin:** Feature 007: Feature Toggle System.
- Review the flag inventory, owners, environment defaults, expiry/review dates, and retirement criteria; the historical inventory below is overdue for review and is not current approval.

| Flag | Recorded owner | Recorded environments | Review date | Retirement trigger |
|---|---|---|---|---|
| `StrategicPlan` | Product + Strategic Maintainer | Development, Test, Production: OFF | 2026-06-30 | Product-ready launch approved and rollout stabilized |
| `DatabaseMigrationsApplyAtStartup` | Platform Engineering | Development: ON; Test and Production: OFF | 2026-06-30 | Startup migration policy finalized and rollout automation in place |

- The 2026-10-16 hello-world approval assigns `HelloWorldBaseline`, `LightTheme`, `DarkTheme`, and `HelpTheme` to the application maintainer, with review at first reporting-release readiness. Retire the shell transition flag after the baseline replaces the historical shell; review theme flags after rollout stabilizes.
- Verify ON/OFF and missing, stale, and invalid configuration behavior for governed flags, including safe fallback and operator diagnostics. Maintain a flag strategy for new user-facing features and themes; flags do not replace authorization.
- **Related requirements:** [FLG-001 through FLG-006](../global-requirements.md#103-feature-flags-and-delivery-governance).

### Flag operation and future options

- **Origin:** Feature 007: Feature Toggle System.
- Document how operators update flags and whether application or configuration reload is required. The hello-world and standard-theme approval recorded a reload requirement.
- Decide whether a different refresh mechanism, role/cohort targeting, production approval for flag changes, or an administrative UI is needed; these are not currently mandatory infrastructure.
- **Related requirements:** [FLG-001 and FLG-006](../global-requirements.md#103-feature-flags-and-delivery-governance).

## Themes

### Accessibility and visual validation

- **Origin:** Feature 008: Theme Expansion.
- Complete and record keyboard-only, desktop screen-reader, and axe/WAVE checks for the changed theme surfaces; resolve critical/serious findings or document approved exceptions. Verify controls, focus, status states, readability, and reduced-motion behavior across enabled themes. Existing automated token-contrast tests and limited desktop/mobile browser checks do not constitute a complete accessibility review.
- Preserve visual and accessibility evidence for release readiness. The current standard-theme appearance was approved, but confirm authorized HELP asset use and final logo/token mapping separately.
- **Related requirements:** [THM-006, THM-007, and A11-001 through A11-003](../global-requirements.md#101-themes-branding-and-shared-ui); accessibility checks remain applicable release requirements.

### Western motifs

- **Origin:** Feature 008: Theme Expansion.
- Revisit horse/cowboy-inspired motifs for the Western theme; its current implementation uses an earth-toned palette and serif typography. Preserve readability and accessibility.
- **Related requirement:** [THM-005](../global-requirements.md#101-themes-branding-and-shared-ui).

### Theme preview and registry

- **Origin:** Feature 008: Theme Expansion.
- Determine whether the live application, which changes immediately when a theme is selected, sufficiently previews colors, typography, controls, and cards, or whether separate samples are needed.
- Address the documented theme registry requirement: theme names, availability flags, and tokens are currently represented in separate places rather than in a single registry.
- **Related requirements:** [THM-003 and THM-004](../global-requirements.md#101-themes-branding-and-shared-ui).

### Preferences, charts, and governance

- **Origin:** Feature 008: Theme Expansion.
- Consider account-linked theme preferences; current theme and Geocities motion preferences are browser-local and do not follow a user across devices.
- Ensure future charts and data views support every enabled theme and remain readable without animation; chart parity cannot be fully validated until charts exist.
- Review whether novelty-theme role/environment restrictions and additional theme-governance guidance or reusable tooling are needed. The existing UI foundation instruction already requires new and changed components to support all enabled themes.
- **Related requirements:** [VIS-007](../global-requirements.md#6-visualization-and-infographic-requirements) and [THM-003 through THM-007](../global-requirements.md#101-themes-branding-and-shared-ui).

## Persistence

The product owner reported on 2026-11-18 that PiDB is working in production;
this is not independent verification of the database schema, backups, recovery,
or detailed configuration. See the tracked [database guidance](../database.md).

### PostgreSQL tests and migration safety

- **Origin:** Feature 009: PostgreSQL & EF Core Integration.
- Test Ticket CRUD, ordering, relevant invalid inputs, constraints, and transactional behavior against an isolated PostgreSQL database. The existing in-memory and SQLite tests do not prove PostgreSQL-specific behavior.
- Exercise migration application and supported recovery paths with representative data. The initial down migration drops `Tickets`; do not describe destructive rollback as lossless.
- **Related requirements:** [PST-009 and PST-011](../global-requirements.md#42-existing-persistence-requirements); PostgreSQL-specific verification remains a release requirement.

### Backups, recovery, and production configuration

- **Origin:** Feature 009: PostgreSQL & EF Core Integration.
- Define, document, and exercise backup and restore, retention, recovery targets, and a reviewed production migration procedure. Verify schema compatibility and data recovery rather than assuming an image rollback restores the database.
- Review PiDB permissions and TLS, verify the production connection and intended database, and validate pool, timeout, lifetime, and resource settings against PiTony/PiDB capacity.
- **Related requirements:** [PST-006, PST-007, PST-009, and PST-010](../global-requirements.md#42-existing-persistence-requirements), plus [QUA-008](../global-requirements.md#9-accessibility-ux-and-operational-quality).

### Future data models and governance

- **Origin:** Feature 009: PostgreSQL & EF Core Integration.
- Add the Director's Report model with Feature 010. Add shared board/work-item persistence when the on-hold Features 003/004 resume; do not introduce a separate strategic-only persistence model.
- Decide whether optimistic concurrency, audit history, soft deletes, further query optimization, or additional EF/migration governance tooling is needed as those features mature.
- **Related requirements:** [PST-005](../global-requirements.md#42-existing-persistence-requirements) and the [Director's Report](feature-010-directors-report.md).
