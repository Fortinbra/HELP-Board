# HELP-Board: Global Application Requirements

Status: Draft for review  
Version: 0.4  
Last updated: 2026-10-16

## 1. Purpose and Authority

HELP-Board is a reporting and data visualization dashboard for the HELP board of directors. It brings information from multiple sources into an understandable, trustworthy view so board members can review performance, identify trends, and make informed decisions.

This document defines application-wide product requirements and architectural constraints. It is not a claim that these capabilities are already implemented, nor is it a release commitment for every planned capability.

Requirement classifications:

- **Confirmed**: Explicitly requested or confirmed by the product owner.
- **Baseline**: Derived quality, security, or usability requirements, including existing repository standards, to review with the product owner.
- **Planned**: Supporting or future scope, not an initial-release commitment.

Each requirement has a stable identifier for feature and test traceability. Implementation details belong in the [feature documents](features/README.md). Feature documents refine these requirements but must not silently change confirmed global scope. Conflicts must be resolved explicitly and reflected in both documents.

## 2. Confirmed Product Decisions

| ID | Requirement | Classification |
|----|-------------|----------------|
| GLB-001 | The primary audience shall be the HELP board of directors, and the primary purpose shall be interactive reporting and data visualization. | Confirmed |
| GLB-002 | An ASP.NET Core Web API shall serve the application API and host the Blazor WebAssembly frontend. | Confirmed |
| GLB-003 | The application-owned database shall be PostgreSQL, accessed through Entity Framework Core. | Confirmed |
| GLB-004 | The application shall support multiple data sources over time, beginning with its own database. | Confirmed |
| GLB-005 | Google Workspace credentials already held by board members shall provide primary sign-in, using OpenID Connect for authentication and OAuth 2.0 where applicable. Authentik OIDC fallback shall also be supported. | Confirmed |
| GLB-006 | A broad, extensible catalog of graphs and infographic types shall be delivered in phases. The initial-release chart list remains to be agreed. | Confirmed |
| GLB-007 | The current reporting scope shall be interactive dashboards. Downloadable reports, exports, and print-specific report delivery are future scope. | Confirmed |
| GLB-008 | Strategic plan tracking and project boards shall remain supporting planned features rather than displacing reporting as the primary purpose. | Confirmed |
| GLB-009 | The production application's canonical public URL shall be `https://help.becauseimclever.com`. Public reachability shall not imply public access to board data. | Confirmed |
| GLB-010 | This is a one-off, purpose-built application for HELP, not a general-purpose dashboard product, multi-tenant platform, or externally distributed API. | Confirmed |
| GLB-011 | The sole production application target shall be a Docker container on PiTony, the owner's Raspberry Pi 5 with 16 GB RAM. Alternative production deployment platforms are out of scope. | Confirmed |
| GLB-012 | The standard themes shall be VS Code-inspired Light and Dark, plus a separate HELP purple/teal theme. Initial selection shall follow the system light/dark preference until the user explicitly chooses a theme. | Confirmed |
| GLB-013 | Geocities, Minimalist, and Western shall remain optional planned themes, not required default themes. | Confirmed |
| GLB-014 | The first business report shall be the Director's Report, generated from a director-completed form stored in the application database, with one report per board meeting, between-meeting updates, and historical trends. | Confirmed |

## 3. Users and Access Responsibilities

Board members are the primary consumers of dashboards and reports. Additional responsibilities below describe capabilities, not an approved role hierarchy:

| Responsibility | Intended capabilities | Status |
|----------------|-----------------------|--------|
| Board/report viewer | Sign in, view authorized dashboards, filter data, inspect visualizations and supporting details. | Confirmed primary audience |
| Director/report author | Complete the Director's Report form, save drafts, and explicitly publish updates for authorized board members. | Confirmed; exact permission assignments remain open |
| Dashboard/report curator | Define approved metrics, dashboard content, and visualization configuration. | Proposed; role assignment and authoring UX undecided |
| Data contributor | Maintain application-owned records used by approved reports, where an editing feature is authorized. | Planned supporting capability |
| Application administrator | Manage access, identity-provider configuration, and approved data-source configuration. | Baseline operational responsibility |

These responsibilities may be combined or separated after approval of the permission model. A successful identity-provider login alone shall not imply permission to view board data.

## 4. Architecture and Persistence

| ID | Requirement | Classification |
|----|-------------|----------------|
| ARC-001 | The browser shall obtain application data through authorized API endpoints, never through direct PostgreSQL access. | Baseline |
| ARC-002 | Database credentials, identity-provider secrets, and future connector credentials shall remain server-side and outside source control and WebAssembly assets. | Baseline |
| ARC-003 | API, service, repository, abstraction, and contract responsibilities shall follow the existing solution boundaries. Reporting logic shall not be embedded in UI markup. | Baseline |
| ARC-004 | Application data and approved persistent configuration shall survive application restarts and deployments through PostgreSQL persistence. | Baseline |
| ARC-005 | Schema changes shall use version-controlled EF Core migrations with a documented deployment and recovery procedure. Production migration execution shall be an explicit operational decision. | Baseline |
| ARC-006 | Constraints, relationships, and transactional writes shall preserve data integrity. EF entities shall not serve as unrestricted public API contracts. | Baseline |
| ARC-007 | Reporting queries shall support bounded results and server-side filtering/aggregation appropriate to the metric, rather than transferring entire datasets to the browser. | Baseline |
| ARC-008 | New source integrations shall use documented contracts so dashboards do not depend on source-specific storage or transport details. External sources are not required to use EF Core or be copied wholesale into PostgreSQL. | Baseline |
| ARC-009 | Production hosting shall provide DNS resolution and a valid TLS certificate for `help.becauseimclever.com`. The hosted frontend and application API shall be available through the canonical HTTPS origin. | Baseline |
| ARC-010 | Production application links and identity-provider redirect configuration shall use the canonical public origin. Local development/test settings shall remain separate; a staging deployment is not required. Any reverse proxy shall preserve the externally visible HTTPS scheme and host through trusted configuration. | Baseline |
| ARC-011 | The API and hosted frontend shall be released as one application. External-client API versioning, parallel supported API versions, public SDKs, and compatibility/deprecation windows are not required. Internal contracts shall remain type-safe and consistent within each release. | Confirmed |

The current solution targets .NET 10. This records the existing baseline, not a requirement to freeze framework versions indefinitely. Supported runtime and dependency upgrades must preserve the confirmed architecture.

### 4.1 Fixed deployment and resource boundaries

| ID | Requirement | Classification |
|----|-------------|----------------|
| OPS-001 | Build and deployment shall produce a Docker image compatible with PiTony's installed OS/CPU platform. Confirm the host's actual platform and use Linux ARM64 when running a 64-bit Raspberry Pi OS; do not assume an x64 image will run. | Baseline |
| OPS-002 | Container configuration shall inject database connectivity, identity-provider configuration, and secrets without rebuilding the image or embedding secrets in image layers. | Baseline |
| OPS-003 | The container shall have documented startup, restart, health-check, logging, update, and rollback procedures on PiTony. Host/container restarts shall not lose persistent application data. | Baseline |
| OPS-004 | CPU, RAM, connection pools, cache size, and background refresh concurrency shall be measured and bounded for PiTony's shared resources. The host's full 16 GB shall not be assumed available to this application. | Baseline |
| OPS-005 | Persistence shall continue to use the documented PiDB PostgreSQL server unless explicitly changed. Hosting the application container on PiTony does not move PostgreSQL or Authentik into that container. | Baseline; existing data topology |
| OPS-006 | Production verification shall exercise the actual PiTony container, database reachability, public HTTPS routing, hosted WASM assets/deep links, and authentication redirects. | Baseline |
| OPS-007 | Release tags and image identifiers shall support deployment traceability and rollback. This operational release identification is not external API versioning. | Baseline |

Kubernetes, cloud deployment variants, multi-region/high-availability topology, deployment portability frameworks, and multiple production hosting targets are not requirements. Local development and test execution remain supported without implying alternative production deployments. Host OS, image platform, Docker runtime settings, reverse-proxy/TLS arrangement, and resource limits must be recorded before deployment.

### 4.2 Existing persistence requirements

These requirements consolidate the [Feature 009 persistence plan](features/feature-009-postgres-efcore-integration.md). Its proof-of-concept scope and historical examples are not evidence of production completion.

| ID | Requirement | Classification |
|----|-------------|----------------|
| PST-001 | Use scoped Npgsql-backed `AppDbContext` and repository registrations through `AddRepositories`. Missing required connection configuration shall fail explicitly. Keep mapping in a sealed context with explicit mapped properties, not shadow properties or query logic. | Baseline |
| PST-002 | Use segregated reader/writer repository interfaces, async I/O with propagated cancellation, explicit saves for writes, and no-tracking reads unless tracking is needed. Cache above repositories, preserving authorization and freshness. | Baseline |
| PST-003 | Persist domain GUID keys, UTC timestamps, required fields, lengths, enum conversions, foreign keys, and documented cascade behavior. Apply ownership/update metadata where required by the domain; future metadata is not assumed already implemented. | Baseline |
| PST-004 | Preserve the Ticket CRUD proof of concept and its documented Title (200), Description (4000), and CreatedBy (100) character limits. Ticket data is not automatically the first approved business reporting dataset. | Baseline |
| PST-005 | Support future boards and typed strategic work items through the shared board/work-item model. Placeholder examples naming separate strategic entities do not authorize a separate strategic-only persistence model. | Planned |
| PST-006 | Store connection settings under `ConnectionStrings:HelpBoard` / `ConnectionStrings__HelpBoard`, with secrets supplied securely. Use UTF-8 PostgreSQL storage and a least-privilege application database user, not a superuser. | Baseline |
| PST-007 | Document explicit pool/timeout/lifetime configuration and validate it against PiTony/PiDB capacity. Feature 009's proposed pool minimum 10, maximum 30, command timeout 30 seconds, and connection lifetime 5 minutes are tuning inputs, not Npgsql defaults or validated production limits. | Baseline |
| PST-008 | Keep transactions short; use normal transactional saves and explicit transactions with rollback for coordinated multi-repository writes. Do not hold transactions across user interaction. | Baseline |
| PST-009 | Review and retain ordered migration history; deployed history shall not be rewritten or deleted. Use new migrations for changes and document create/apply/script/recovery workflows, backup requirements, and data-loss implications. Test migration application and supported down/up recovery paths against expected data semantics; destructive rollback shall not be presented as lossless. Prefer application initialization over migration seeding unless explicitly justified. | Baseline |
| PST-010 | `DatabaseMigrationsApplyAtStartup` shall default OFF for production and tests. Development may explicitly opt in. Production changes require reviewed migration execution; an image rollback alone shall not be assumed to undo schema/data changes. | Baseline |
| PST-011 | Define request, service/business-rule, repository, and database constraint validation boundaries. Test CRUD roundtrips, sorting/filtering, not-found, invalid data, and transactional behavior. In-memory tests shall not be treated as proof of PostgreSQL constraints or migration correctness. | Baseline |

Full-text search, advanced JSONB, sharding/read replicas, general audit-history/soft-delete infrastructure, concurrency policy, and advanced migration scenarios remain future or undecided scope. Authentication audit events and strategic latest-update attribution are still required within their defined scope.

## 5. Dashboards and Interactive Reporting

| ID | Requirement | Classification |
|----|-------------|----------------|
| REP-001 | Authorized board members shall be able to view consolidated indicators and visualizations for approved reporting questions. | Confirmed |
| REP-002 | Each production dashboard shall identify its purpose, intended audience, and the metrics it presents. | Baseline |
| REP-003 | Reports shall support applicable date-range and domain filters. Available filters and default periods shall be specified per report rather than assumed globally. | Baseline |
| REP-004 | A visualization shall show its title, metric meaning, units, reporting period, and active filter context. | Baseline |
| REP-005 | Where authorized supporting detail exists, users shall be able to move from a summary visualization to the corresponding detail without losing reporting context. | Baseline |
| REP-006 | Loading, empty, unavailable, stale, partially available, and failed states shall be distinguishable. An unavailable metric shall never appear as a genuine zero or a successful result. | Baseline |
| REP-007 | A failed source or widget shall not prevent independent, available widgets from rendering. Any partial result shall be labeled clearly. | Baseline |
| REP-008 | Users shall be able to request a refresh; each source/report shall define its refresh behavior and show its last successful data update. | Baseline |
| REP-009 | Source data timestamps and retrieval timestamps shall be distinguished where both exist. Refreshing a view shall not falsely imply that the underlying data changed. | Baseline |
| REP-010 | Historical trends shall use actual timestamped observations or explicitly defined snapshots. Current values shall not be presented as historical facts. | Baseline |
| REP-011 | Use a responsive dashboard shell with summary KPIs, widget/card regions, and applicable filters. A widget contract shall identify its key, title, metrics, source, refresh strategy, states, and drill-down destination, independent of source technology. | Baseline; Feature 002 |
| REP-012 | Prioritize above-the-fold summaries, progressively load lower-priority widgets, and debounce frequent filter changes. Any caching shall respect freshness and user permissions. | Baseline; Feature 002 |

Saved views, personal dashboard layouts, self-service report building, scheduled delivery, and public sharing are not implied by interactive reporting. Their inclusion requires separate scope approval.

### 5.1 First report: Director's Report

The [Director's Report feature plan](features/feature-010-directors-report.md) elaborates these requirements. Field definitions are undecided; no financial, personnel, participant, or other domain-specific metrics are approved by implication.

| ID | Requirement | Classification |
|----|-------------|----------------|
| DIR-001 | Provide a director-facing form whose persisted inputs generate the first interactive Director's Report. Approved numeric/date/categorical fields shall support appropriate visualizations; narrative content shall remain readable as narrative rather than invented quantitative metrics. | Confirmed purpose; baseline presentation |
| DIR-002 | Associate one logical Director's Report with each board meeting. Support 12 regular board meetings per year plus additional ad hoc meetings without an annual cap of 12 reports. | Confirmed |
| DIR-003 | Identify meetings independently of calendar month or date, with regular/ad hoc type and a scheduled date. Rescheduling shall retain the meeting/report identity; multiple meetings in one month shall be distinguishable. Exact schedule, timezone, and canceled-meeting policy remain open. | Baseline |
| DIR-004 | The director shall save drafts and explicitly publish initial reports and later updates. Board members shall continue to see the last successfully published version, not unpublished edits; an unpublished report shall not appear as a completed report. | Confirmed |
| DIR-005 | Support ongoing updates to the live report for the upcoming meeting between board meetings. Display the target meeting, reporting period/as-of context, author, last saved/published timestamps as appropriate to permissions, and whether a later draft awaits publication. | Confirmed ongoing updates; baseline metadata |
| DIR-006 | Published reports shall be visible to all authorized board members, not to unauthenticated public visitors. Draft editing/publication shall require explicit authoring permissions, enforced by the API. | Confirmed |
| DIR-007 | Preserve a fixed snapshot of the report for each meeting. Subsequent corrections shall be recorded separately and linked to that snapshot, without overwriting what the board saw. Snapshot timing/authority and correction publication workflow must be agreed. | Confirmed |
| DIR-008 | Retain attributable published revisions, meeting snapshots, and linked corrections with their timestamps and source values so prior publications can be understood. Updates to the next meeting's report shall not mutate earlier meeting records. | Baseline supporting confirmed history |
| DIR-009 | Provide a meeting-based archive and historical trend views across comparable approved metrics. Distinguish meeting date, measurement period, and publication time; do not assume every report represents a unique calendar month. | Confirmed history; baseline comparison rules |
| DIR-010 | Ad hoc reports and overlapping periods shall not automatically be summed into monthly/annual totals or counted as extra observations of the same measured period. Define trend inclusion, correction treatment, comparable units, and missing-data rules per metric. | Baseline |
| DIR-011 | Report generation/publication shall use validated persisted form data and preserve consistency between narrative, values, charts, and revision identity. Failed saves/publications shall be explicit and shall not replace the last good publication or lose a meeting snapshot. | Baseline |
| DIR-012 | Form and metric-definition changes shall preserve interpretation of earlier reports. Retain the definition/unit/period context needed to explain historical values; this internal history requirement does not introduce external API versioning. | Baseline |

Drafts are editable working content; a published revision is what viewers can currently see; a meeting snapshot is the preserved meeting record; a correction is a separate attributable amendment. A later unsubmitted draft shall never be silently substituted into the meeting record. How the next meeting/report is created and whether prior values are carried forward remain open.

## 6. Visualization and Infographic Requirements

### 6.1 Breadth and phased delivery

Breadth is a product requirement, not a mandate to put every chart on every dashboard. Select visuals that accurately answer the reporting question and fit the data.

The following is a candidate catalog for phased planning, not an approved initial-release checklist:

| Reporting need | Candidate visualization types |
|----------------|-------------------------------|
| Summary and change | KPI/stat cards, comparison cards, sparklines, trend arrows with numeric change |
| Category comparison | Horizontal/vertical bars, grouped bars, stacked bars, 100% stacked bars, dot/lollipop plots |
| Trends over time | Line, multi-series line, step line, area, stacked area, calendar heatmap |
| Composition | Donut/pie for limited categories, treemap, proportional stacked bars, waffle/pictogram charts |
| Distribution | Histogram, box plot, density plot |
| Relationships | Scatter plot, bubble plot, correlation heatmap |
| Targets and progress | Progress bars, bullet charts, target-versus-actual indicators, clearly labeled gauges |
| Change and flow | Waterfall, funnel, Sankey/alluvial flow diagrams |
| Schedule and strategy | Milestone timelines, Gantt-style views, hierarchy/tree diagrams |
| Geographic patterns | Choropleth and point/bubble maps, only when approved geographic data is available |
| Supporting detail | Accessible data tables, ranked lists, annotated narrative/infographic panels |

| ID | Requirement | Classification |
|----|-------------|----------------|
| VIS-001 | Visualization support shall be extensible across the catalog without redesigning the dashboard or its source contracts for each new type. | Confirmed |
| VIS-002 | Each delivery phase shall approve a specific supported-type list. The first production release shall not be called visualization-complete until its approved list passes acceptance checks. | Baseline |
| VIS-003 | Each supported type shall declare its accepted data shape, required fields, supported interactions, and treatment of missing, negative, zero, and high-cardinality values. | Baseline |
| VIS-004 | Applicable visuals shall support legends, clear labels, units, accessible value inspection, and filtering/drill-down where meaningful. Unsupported interactions shall not be implied. | Baseline |
| VIS-005 | Scales, baselines, aggregation, sorting, and comparisons shall not distort meaning. Truncated axes and sampling shall be disclosed; bar magnitude comparisons shall use a zero baseline. | Baseline |
| VIS-006 | Missing, suppressed, invalid, and zero values shall remain distinguishable in visuals and supporting tables. | Baseline |
| VIS-007 | Visualizations shall use consistent metric formatting and semantic colors across dashboards and supported themes. They shall remain usable without animation. | Baseline |
| VIS-008 | Charting technology shall be evaluated for catalog coverage, Blazor WASM integration, accessibility, performance, maintenance, and licensing before selection. Multiple libraries require explicit justification. | Baseline |

No charting vendor or commercial license is selected by this document. The in-repository UI design system remains the shared application shell and control foundation; choosing a chart engine is a separate decision.

## 7. Data Sources, Metrics, and Trust

| ID | Requirement | Classification |
|----|-------------|----------------|
| DAT-001 | The first production reporting integration shall read approved application-owned PostgreSQL data through EF Core-backed server-side data access. | Confirmed |
| DAT-002 | Every production source shall have a documented owner, access method, dataset definition, sensitivity classification, refresh policy, and failure behavior. | Baseline |
| DAT-003 | Every metric shall define its business meaning, source fields, formula, units, aggregation grain, applicable filters, and handling of missing values. | Baseline |
| DAT-004 | Time-based metrics shall define reporting timezone, period boundaries, and calendar/fiscal-year assumptions. Currency and percentage calculations shall define rounding and denominator rules. | Baseline |
| DAT-005 | Cross-source joins shall define identity mapping, deduplication, compatible time periods, and reconciliation rules before combining values. | Baseline |
| DAT-006 | Validation failures, incomplete ingestion, and stale data shall be observable to operators and surfaced to viewers where they affect interpretation. | Baseline |
| DAT-007 | Calculations and roll-ups shall have deterministic tests against known inputs and expected outputs. Dashboard totals and supporting detail shall reconcile under the same filters. | Baseline |
| DAT-008 | Future integrations shall be independently scoped and approved, including their access rights, operational costs, rate limits, and data-handling obligations. | Planned |

The first database-backed business dataset is the director-entered Director's Report. Its form sections, reporting questions, metric definitions, and chart selections remain open. Existing ticket persistence is a technical foundation, not the first approved business report.

## 8. Authentication, Authorization, and Privacy

| ID | Requirement | Classification |
|----|-------------|----------------|
| SEC-001 | Users shall sign in primarily with their existing Google Workspace identity. The application shall not introduce its own password store. | Confirmed |
| SEC-002 | Authentik shall provide a supported OIDC fallback with documented activation, account eligibility, and recovery procedures. Fallback shall not bypass normal authorization or create an unrestricted emergency login. | Confirmed |
| SEC-003 | The API shall enforce authorization independently of frontend route guards, chart visibility, or feature flags, including supporting-detail and future export endpoints. | Baseline |
| SEC-004 | Access shall be restricted to approved organizational identities and application permissions. Email domain hints alone shall not authorize a user; Workspace membership and eligibility rules must be defined and validated server-side. | Baseline |
| SEC-005 | Authentication shall use a standards-compliant OIDC flow. A browser public-client flow shall use authorization code with PKCE and no client secret; any server-held credentials shall remain server-side. | Baseline |
| SEC-006 | Token validation shall enforce the applicable issuer, audience, signature, and lifetime checks; login flows shall validate state and nonce as applicable. API credentials shall be intended for the API, not accepted merely because they are Google identity tokens. | Baseline |
| SEC-007 | External identities shall be keyed by issuer/provider and subject. Google and Authentik accounts shall not be automatically linked solely by matching email addresses. Linking and fallback access require an approved mapping procedure. | Baseline |
| SEC-008 | The permission model shall define provisioning, default access, administrator bootstrap, role changes, deprovisioning, and revocation behavior across both providers. Least privilege shall be the default. | Baseline |
| SEC-009 | HTTPS, approved redirect destinations, secure session/token handling, sign-out, expiry, and reauthentication behavior shall be defined and tested. Cookie-based designs shall include CSRF defenses for state-changing operations. | Baseline |
| SEC-010 | Authentication failures, denied access, provider transitions, and administrative access changes shall be auditable without logging tokens, secrets, or unnecessary personal data. | Baseline |
| SEC-011 | Reports shall expose only data authorized for the requesting user. Aggregation, drill-down, and caching shall preserve those restrictions. | Baseline |
| SEC-012 | Google Workspace and Authentik registrations shall allow only explicitly approved authentication callback URLs. Production callbacks shall use `https://help.becauseimclever.com` with paths defined by the approved authentication design; callback paths are not selected by this document. | Baseline |
| SEC-013 | Retain the approved identity profile fields (provider/subject, email, display name, last login, provisioning source) and auditable role/permission assignments. Provide clear sign-in/redirect, provisioning, access-denied, session status, and provider-unavailable states with support guidance. | Baseline; Feature 005 |

Whether fallback activation is manual or automatic, whether provider selection is visible to users, and the session architecture remain open design decisions. Both providers must enforce the same application access policy.

## 9. Accessibility, UX, and Operational Quality

| ID | Requirement | Classification |
|----|-------------|----------------|
| QUA-001 | New dashboard and visualization experiences shall meet WCAG 2.2 AA, including keyboard operation, visible focus, semantic structure, accessible names, and contrast. | Baseline; existing UI standard |
| QUA-002 | Each chart shall provide an accessible textual summary and an equivalent accessible data view. Information shall not depend on color, hover, animation, or pointer precision alone. | Baseline |
| QUA-003 | Layouts shall be responsive and usable at supported zoom levels. Dense views shall retain readable labels and accessible navigation rather than shrink indefinitely. | Baseline |
| QUA-004 | Shared UI shall follow the existing design tokens, in-repository components, icon wrapper, and code-behind policy. New UI shall not introduce Bootstrap dependencies into migrated surfaces. | Baseline; existing UI standard |
| QUA-005 | Each production dashboard shall have approved and measured response/render budgets, expected data volume, and concurrent-user workload. Numeric targets must be agreed before performance acceptance. | Baseline |
| QUA-006 | Large results shall use bounded queries and appropriate aggregation, pagination, or disclosed sampling. Timeouts and cancellation shall prevent unbounded work. | Baseline |
| QUA-007 | Source outages, database failures, and API errors shall produce actionable user states and structured operator diagnostics without leaking sensitive implementation details. | Baseline |
| QUA-008 | Production operation shall include health monitoring, protected environment configuration, backup/restore procedures, and tested deployment/recovery steps. Availability, retention, and recovery targets remain to be approved. | Baseline |
| QUA-009 | Automated checks shall cover metric logic, PostgreSQL-specific persistence behavior, API permissions, provider flows, and key dashboard journeys. Accessibility checks shall include manual keyboard and assistive-technology review. | Baseline |
| QUA-010 | Placeholder/demo data shall be visibly identified and shall not be represented as live organizational reporting. Feature flags shall not be treated as authorization controls. | Baseline |

## 10. Delivery Boundaries and Acceptance

### Initial hello-world milestone

This is the first visual baseline, preceding the first production reporting release. It deliberately contains no live reports or working login. It does not waive authentication or reporting acceptance for production board data.

| ID | Requirement | Classification |
|----|-------------|----------------|
| BSL-001 | Show a header across the top, a left navigation sidebar, and a main content region containing simple sample/welcome text. On narrow screens, stack navigation above content without losing keyboard access or causing horizontal overflow. | Confirmed layout; baseline responsive behavior |
| BSL-002 | Place the locally hosted official HELP logo and application identity in the upper-left header corner; place the theme selector and login status at the far upper right. | Confirmed |
| BSL-003 | Show Director's Report, Strategic Plan, and Budget Report as sample navigation items that do not navigate. Mark them unavailable/coming soon accessibly rather than sending users to empty or misleading routes. Budget Report is an illustrative label, not approval of its fields or implementation scope. | Confirmed |
| BSL-004 | Provide functional System, Light, Dark, and HELP theme selection, following system preference until overridden and retaining choice in browser-local storage for this baseline. Account-linked preference persistence is deferred until identity is implemented. Surface storage failures rather than implying persistence succeeded. | Baseline implementation of confirmed theme direction |
| BSL-005 | Display honest unsigned-in/login-unavailable status; do not simulate authenticated identity or offer a working-looking sign-in button without authentication. No director form, API data loads, graphs, or real report links are required in this milestone. | Baseline scope |
| BSL-006 | Retain existing groundwork behind a rollout switch. `HelloWorldBaseline` and the bundled `LightTheme`, `DarkTheme`, and `HelpTheme` flags are explicitly approved ON for this milestone by the product owner's request on 2026-10-16. `StrategicPlan` remains OFF: the disabled sample label is not an enabled strategic route or launch approval. | Confirmed milestone activation; baseline governance |

Milestone checks: verify desktop layout and responsive stacking, logo loading without runtime website access, disabled navigation behavior, truthful login status, keyboard focus/skip link, system-theme changes, explicit theme override, browser reload persistence, and theme failure/disabled-state behavior. These checks establish the visual baseline only, not completion of all global requirements.

### 10.1 Themes, branding, and shared UI

| ID | Requirement | Classification |
|----|-------------|----------------|
| THM-001 | Provide Light and Dark themes inspired by VS Code's built-in default light/dark visual language, plus a distinct HELP theme using HELP purple and teal. This does not require copying the VS Code interface or adopting its branding. | Confirmed |
| THM-002 | With no explicit user choice, follow the system light/dark preference, including changes to that preference. A saved explicit theme selection shall take precedence until the user returns to system selection. | Confirmed |
| THM-003 | Provide a theme selector and preview of key components. Changes shall apply immediately in-session, persist as user preferences, and load on subsequent sign-in/session start. Define a safe available-theme fallback if a selected theme is disabled. | Baseline; Feature 008 |
| THM-004 | Represent themes through a registry of key, display name, token set, and availability flag. All themes shall preserve core controls, states, chart readability, and semantic status meanings. | Baseline |
| THM-005 | Retain optional Geocities (bright/nostalgic), Minimalist (low-noise), and Western (horse/cowboy-inspired) plans behind separate flags. Decorative effects/fonts shall preserve legibility; reduced motion shall suppress nonessential animation. | Planned; confirmed retention |
| THM-006 | Use the official HELP website as the source for HELP logos and brand color references. Record provenance and confirm authorized asset use and final token mappings; do not hotlink runtime branding assets or make application startup depend on the website. | Confirmed brand source; baseline asset handling |
| THM-007 | Adapt brand reference colors into contrast-tested semantic tokens for backgrounds, text, controls, focus, and charts. Raw brand colors are not automatically accessible foreground/background pairs. | Baseline |
| UIF-001 | Use the in-repository Blazor design system rather than a replacement UI framework. Central CSS tokens shall cover colors, spacing, radii, elevation, typography, and interaction states; use semantic class names and reusable components instead of duplicated page-specific styles. | Baseline; Feature 001 |
| UIF-002 | Preserve shell header/navigation/content regions, cards/panels/page headers, buttons, inputs, toggles, tabs, alerts, badges, progress indicators, and hover/active/focus/disabled/loading/empty/error states. Body text shall use the documented 16 px minimum baseline with ordered heading scales. | Baseline |
| UIF-003 | Standardize Lucide icons through `AppIcon` only. Hide decorative icons from assistive technology, name meaningful/icon-only actions, and pair status icons with descriptive text. Exceptions require a documented reason and follow-up issue. | Baseline |
| UIF-004 | Keep behavior in paired `.razor.cs` partial classes, with `.razor` limited to markup/directives; static presentation-only components need no code-behind. | Baseline |
| UIF-005 | Complete Bootstrap retirement through inventory/mapping, layout/navigation, controls/feedback, then asset removal. New surfaces shall not introduce Bootstrap classes. Temporary migration exceptions require an inline removal comment and tracking issue. Validate and retain phase-local rollback evidence. | Baseline |
| A11-001 | Preserve skip-to-main navigation, route-change focus, logical reading/tab order, stable focus during expand/collapse and modal/inline operations, semantic landmarks, and heading hierarchy. Announce passive updates and urgent errors appropriately. | Baseline |
| A11-002 | Meet contrast thresholds: normal text at least 4.5:1, large text at least 3:1, and meaningful non-text controls/indicators at least 3:1 against adjacent colors. Validate actual theme combinations rather than assuming brand colors pass. | Baseline |
| A11-003 | Require keyboard-only and desktop screen-reader smoke checks plus axe/WAVE checks for changed UI. Critical/serious blockers shall be resolved or have a documented approved exception with owner/date; preserve visual regression and accessibility evidence for each migration/theme phase. | Baseline |

**Brand references inspected on 2026-10-16:**

- [Official HELP website](https://www.helpcenterofaustin.org/).
- [Published HELP logo, labeled "Correct Colors" in its filename](https://static1.squarespace.com/static/5c32c0bd5ffd200348050279/t/679920a66510b00378ee444d/1738088614083/H.E.L.P.+Logo+Correct+Colors+-+transp+bkgrd_2022.png?format=1500w).
- [Website stylesheet inspected for color references](https://static1.squarespace.com/static/sitecss/5c32c0bd5ffd200348050279/94/55f0aac0e4b0f0a5b7e0b22e/5c32c0bd5ffd20034805028e/360/site.css?nocustom=true) contains purple `#7400B3` and teal `#00B4D0`. The published raster logo contains nearby colors (sampled dominant saturated pixels `#7300B3` and `#00B5D0`). These are observed source references, not a formal brand guide or approved UI palette.

Feature 001's earlier teal `#0F766E` / purple `#6D28D9` foundation remains historical implementation context, not the authoritative HELP brand palette. Confirm final logo variant and semantic tokens against the official assets before branding implementation. Existing spacing tokens (4, 8, 12, 16, 24, 32, 48 px) and semantic success/warning/error/info roles remain the foundation.

### 10.2 Strategic and project work requirements

The following consolidates supporting planned capabilities without promoting them into the first reporting release:

| ID | Requirement | Classification |
|----|-------------|----------------|
| WRK-001 | Use one shared board/work-item model for project boards and Strategic Plan. Strategic Plan shall be an always-present required system board with reserved identity/name and non-deletable constraints; hiding its UI shall not remove its data invariant. | Planned; Features 003/004 |
| WRK-002 | Model strategic themes/objectives/initiatives/milestones as typed work items with explicit hierarchy links. Objective, initiative, and milestone progress rolls up milestone -> initiative -> objective, using equal child weights when explicit weighting is absent. | Planned |
| WRK-003 | Document exact progress/health formulas, tie-breakers, missing/null handling, weighting, and rounding. Tests shall prove objective-level results from child updates; health behavior shall not be inferred solely from percentage progress. | Baseline for planned strategic work |
| WRK-004 | Strategic items shall retain an explicit owner (approved person/role/team identity) and latest `updatedBy`/`updatedAt` metadata on status/progress changes. Full historical audit support is future scope, distinct from mandatory latest-update attribution. | Planned |
| WRK-005 | Support strategic overview, objective detail, linked initiatives/milestones, timeline checkpoints, and ownership panels; represent planned/on-track/at-risk/blocked/completed/archived states as appropriate. | Planned |
| WRK-006 | Provide strategic health, objectives-at-risk, and milestones-due-soon dashboard signals with authorized drill-down. Persist updates immediately; use actual historical observations/snapshots when trend reporting is implemented. | Planned |
| WRK-007 | Support project-board creation from blank/template, board directory/settings, draft/active/archived lifecycle, and configurable statuses, swimlanes, and fields within approved limits. Preserve Strategic Plan reserved constraints. | Planned |
| WRK-008 | Work items shall support identity/board linkage, title/description, owner, status, priority, due dates, progress, and strategic relationships. Planned workflow states include backlog, ready, in progress, blocked, done, and cancelled; customization limits remain open. | Planned |
| WRK-009 | Provide board/detail and applicable kanban/list/table views; define status-history/reporting implications when that supporting feature is delivered. Board workload/progress/health shall contribute to approved dashboard metrics. | Planned |
| WRK-010 | Board drag-and-drop shall have keyboard alternatives. Hierarchies and timelines shall provide predictable navigation, expand/collapse focus behavior, and equivalent text summaries. | Baseline for planned board UI |

### 10.3 Feature flags and delivery governance

| ID | Requirement | Classification |
|----|-------------|----------------|
| FLG-001 | Use a simple OSS/free-friendly, local/self-hosted feature management approach. Existing runtime direction is Microsoft FeatureManagement with `FeatureManagement` configuration; cloud flag control and tenant targeting are not requirements for this one-off application. | Baseline; narrowed to fixed deployment scope |
| FLG-002 | Every new user-facing feature/enhancement and every new theme shall have a flag strategy. Default OFF unless explicit product approval is recorded; test/production differences shall be intentional, not implicit. | Baseline; Feature 007 |
| FLG-003 | `StrategicPlan` shall remain OFF until product-ready approval is recorded. Strategic navigation, board-directory entry points, and dashboard links shall respect that gate; server-side authorization remains independent. | Baseline |
| FLG-004 | Flag records shall include key, description, owner, default, applicable environment settings, review/expiry date, and retirement criteria. Review expired flags and remove obsolete ones. Historical flag inventory dates do not constitute current rollout approval. | Baseline |
| FLG-005 | Test ON/OFF and missing/stale/invalid configuration behavior. Fall back to approved safe states with operator diagnostics, not silent success; disabled themes shall not appear as selectable. The standard theme set must receive explicit enablement approval before release. | Baseline |
| FLG-006 | Document how operators change flag values and whether reload/restart is required. An admin flag-management UI, user targeting, and no-restart refresh mechanisms remain design options rather than mandatory new infrastructure. | Baseline |
| CIC-001 | PR checks shall build and run all tests before merge. Protect the default branch with required checks, an up-to-date branch, no direct pushes, at least one required review, and dismissal of stale approvals after new commits. | Baseline; Feature 006 |
| CIC-002 | A published release/tag shall trigger checkout, restore, build, full tests, ARM-compatible container packaging, and controlled deployment to PiTony through a trusted self-hosted runner/agent inside the private network. The runner need not itself run on PiTony. | Baseline |
| CIC-003 | Inject deployment secrets through protected GitHub configuration; isolate trusted deployment execution from untrusted PR code. Production deployment shall require explicit approval, pre-deploy validation, and post-deploy health checks. | Baseline |
| CIC-004 | Record release/image identity, target, timestamp, operator, and result. Document and exercise recovery to the previous known-good container plus a schema/data recovery plan; automatic versus approval-based rollback remains open. | Baseline |
| DEV-001 | Preserve SOLID, Clean Code, dependency injection, nullable types, modern supported .NET/C#, structured logging, cancellation propagation, and no sync-over-async. Follow existing naming, validation, and localization patterns. | Baseline; repository standards |
| DEV-002 | Write deterministic tests before or alongside behavior, using existing xUnit/assertion/mocking patterns and scenario-based names with Arrange/Act/Assert structure. Unit tests shall isolate external systems; integration checks shall verify real infrastructure behavior where required. | Baseline |
| DEV-003 | Each feature change shall document its risks, dependencies, acceptance criteria, and verification evidence. Identify necessary instructions, reusable prompts, or skills when a new recurring rule warrants them; candidate governance suggestions are not automatically requirements to create every proposed customization. | Baseline |

The existing RICE prioritization method remains a planning aid: `(Reach x Impact x Confidence) / Effort`, with recorded inputs and tiers reviewed as scope changes. Historical scores do not waive production authentication, baseline themes, or release acceptance. This document does not introduce external-client API lifecycle management.

### 10.4 Initial production reporting capability

The first reporting release shall demonstrate the following, with its exact business dataset and chart list approved before implementation acceptance:

1. The Docker container on PiTony serves the Blazor WASM application and its API at `https://help.becauseimclever.com`, with working DNS and valid HTTPS.
2. Eligible board users can sign in with Google Workspace; ineligible users and unauthorized direct API requests are denied.
3. Authentik fallback is exercised under the approved activation and identity-mapping procedure, preserving equivalent permissions.
4. The Director's Report form persists real data in PostgreSQL through EF Core-backed server-side writes/queries and generates an authorized interactive report.
5. Every chart type selected for that release renders its approved data shape correctly, including documented edge cases and accessible equivalents.
6. Filters, calculations, timestamps, and supporting detail reconcile against agreed reference data.
7. Empty, stale, failed, and partially available results are demonstrably distinct from valid zero values.
8. Approved performance and accessibility acceptance checks pass.
9. Operational configuration, migrations, backup/restore, and support procedures are documented and exercised to the agreed release scope.
10. Light/Dark system selection, explicit persisted choice, and the HELP theme pass theme, chart, and accessibility checks. Optional novelty themes remain separately gated.
11. Approved resource limits are validated on PiTony, and release deployment/recovery is exercised on that sole production target.
12. Draft/publish isolation, between-meeting updates, meeting snapshots, linked corrections, and history/trend comparisons pass the Director's Report acceptance checks. Verify 12 regular meetings plus additional ad hoc meetings without overwriting or double-counting history.

These are acceptance expectations, not statements of current completion. Required datasets, chart selections, and measurable operating targets are release-planning blockers until agreed.

### 10.5 Subsequent phases

- Expand visualization coverage against the approved catalog.
- Add individually approved external data sources.
- Add strategic/project reporting and supporting work-management features under their existing feature plans and launch gates.
- Evaluate exports, print-specific reports, scheduled delivery, saved views, and self-service authoring through separate requirements.

No external source, refresh interval, export format, role hierarchy, or paid visualization dependency is approved by implication.

## 11. Relationship to Existing Documentation

| Existing document | Relationship |
|-------------------|--------------|
| [Feature 001: UI Foundation](features/feature-001-ui-foundation-and-theme-system.md) | Shared UI and WCAG foundation; does not itself deliver the reporting catalog. |
| [Feature 002: Dashboard Experience](features/feature-002-dashboard-experience.md) | Dashboard groundwork only. Its database exclusions are boundaries of that feature slice, not a global exclusion of database-backed reporting. |
| [Feature 003: Strategic Plan Tracker](features/feature-003-strategic-plan-tracker.md) | Supporting planned tracking/reporting capability with existing ownership, roll-up, and launch-gate constraints. |
| [Feature 004: Project Boards](features/feature-004-project-boards.md) | Supporting planned work-management capability and shared board model. |
| [Feature 005: Authentication](features/feature-005-authentication-and-authorization.md) | Google Workspace primary and required Authentik fallback; detailed eligibility, session, and activation policy still require decisions. |
| [Feature 006: Delivery Pipeline](features/feature-006-ci-cd-and-release-deployment.md) | Validation and deployment planning supporting production operation. |
| [Feature 007: Feature Toggles](features/feature-007-feature-toggle-system.md) | Controlled rollout; not access control. |
| [Feature 008: Themes](features/feature-008-theme-expansion.md) | Standard Light/Dark/HELP themes, system preference and persistence; optional novelty themes retained behind flags. |
| [Feature 009: PostgreSQL and EF Core](features/feature-009-postgres-efcore-integration.md) | Persistence foundation; a separate reporting slice must define business datasets and aggregates. |
| [Feature 010: Director's Report](features/feature-010-directors-report.md) | First form-driven business report, publication lifecycle, meeting records, and historical trends. |
| [Accessibility checklist](accessibility/wcag-checklist.md) and [accessibility decisions](architecture/accessibility-decisions.md) | Contrast thresholds, icon semantics, skip/route focus, status announcements, and manual/automated evidence, consolidated under QUA/UIF/A11. |
| [Bootstrap migration](migration/bootstrap-migration-plan.md) | Phased replacement, exit checks, and phase-local rollback, consolidated under UIF-005. |
| [RICE prioritization](features/feature-prioritization-rice.md) | Planning method and historical estimates, not a substitute for current release gates. |
| [Feature template](features/feature-template.md) and [repository instructions](../.github/instructions/) | Documentation structure and engineering/UI/strategic guardrails, consolidated under DEV/UIF/WRK/A11. |

Coverage summary: GLB/ARC/OPS/PST cover purpose, architecture and persistence; REP/VIS/DAT cover dashboard and reporting; DIR covers Feature 010; SEC covers identity; THM/UIF/A11/QUA cover presentation and accessibility; WRK covers Features 003/004; FLG covers Feature 007; CIC covers Feature 006; DEV covers shared engineering and feature governance. Detailed schemas, examples, historical completion records, risk mitigations, and undecided design options remain in their source documents rather than being reclassified as delivered features or confirmed product decisions.

Repository inspection for this draft confirms the hosted WASM structure, PostgreSQL/EF Core registration, and a placeholder dashboard foundation. It does not establish completion of production reporting, the chart catalog, or authentication.

## 12. Open Decisions

| ID | Decision needed | Affected requirements |
|----|-----------------|-----------------------|
| DEC-001 | Which form sections, fields, board questions, and KPIs define the Director's Report? Who approves field validation and metric definitions? The report identity and director-entered database source are confirmed. | DIR-001, REP-001, DAT-001 through DAT-004 |
| DEC-002 | Which visualization types are required in each phase, especially the first production release? | GLB-006, VIS-001 through VIS-008 |
| DEC-003 | What Workspace organizations/domains and membership checks establish eligibility, and how is access provisioned/revoked? | SEC-004, SEC-008 |
| DEC-004 | What are the approved roles and permissions? Who can curate dashboards or edit source records? | Section 3, SEC-003, SEC-008 |
| DEC-005 | How is Authentik activated, how are fallback identities mapped, and how is fallback tested during a Google outage? | SEC-002, SEC-007 |
| DEC-006 | Which session/token architecture, expiry, idle timeout, and revocation deadline apply? | SEC-005, SEC-006, SEC-009 |
| DEC-007 | Which filters, fiscal calendar, timezone, locale, currencies, and comparison periods apply? | REP-003, DAT-004 |
| DEC-008 | What data freshness, history/snapshot, retention, and sensitivity rules apply to each dataset? | REP-008 through REP-010, DAT-002 |
| DEC-009 | What workloads, response/render thresholds, browser/device support, availability, and recovery targets define acceptance? | QUA-003, QUA-005, QUA-008 |
| DEC-010 | Which future sources and visualization technology meet the approved business, accessibility, licensing, and operational needs? | DAT-008, VIS-008 |
| DEC-011 | What are PiTony's actual OS/image platform, container limits, restart policy, networking, TLS termination, and deployment health thresholds? | OPS-001 through OPS-007, CIC-003 |
| DEC-012 | Which approved HELP logo variant and final contrast-tested tokens should be packaged? Which VS Code light/dark defaults serve as the precise visual references? | THM-001, THM-006, THM-007 |
| DEC-013 | What are board customization limits/template ownership, strategic health confirmation cadence, full-history scope, and concurrency-conflict handling? | WRK-003 through WRK-009, PST-011 |
| DEC-014 | Which flag values are approved for release, how are changes applied, and is rollback automatic or approval-based? | FLG-002 through FLG-006, CIC-004 |
| DEC-015 | Who creates/reschedules meetings, creates the next report, publishes updates/corrections, and freezes the meeting snapshot? What event or explicit action fixes that snapshot, and how are canceled meetings handled? | DIR-002 through DIR-008 |
| DEC-016 | What measurement periods and comparable metrics drive trends? How are ad hoc reports, overlapping periods, snapshots, and corrections selected/labeled without double-counting? Is any carry-forward allowed, and how is it identified as unconfirmed? | DIR-009 through DIR-012 |

## 13. Requirements Maintenance

New implementation features shall reference applicable requirement IDs, state their delivery slice, and map acceptance criteria to tests or documented verification. Unresolved decisions shall remain visible rather than being replaced with silent defaults. Approved changes to global scope shall update this document and directly affected feature documents together.
