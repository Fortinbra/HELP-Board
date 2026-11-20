# Feature 011: Shared Dashboard Builder and Data Sources

Status: Builder implemented; provider integrations not implemented
Last updated: 2026-11-19

The dashboard builder flag is enabled in the current web configuration. Its
database migration must be reviewed and applied to the intended environment
before deployment; it has not been applied to production. This does not waive
authentication, accessibility, backup, recovery, or other release requirements.

## Goal

Allow authorized users to create shared dashboards, arrange and resize widgets on a grid, and eventually select a data source for each data-driven widget. Deliver the builder first without live financial data; PostgreSQL is the initial provider direction, and financial data such as receivables, payables, and donations is expected to come mostly from QuickBooks Online.

This feature refines [GLB-004 and the reporting and integration requirements](../global-requirements.md#4-architecture-and-persistence). The [dashboard requirements](../global-requirements.md#5-dashboards-and-interactive-reporting) include source-agnostic guidance; graph configuration and data-source selection are not yet implemented. This is a purpose-built HELP-Board feature, not a general-purpose dashboard product.

## User Value

Authorized users can create and edit shared dashboards without a dedicated page for each report. Board members can later connect widgets to approved sources and see attributable data without mistaking missing or unavailable data for a genuine value.

## Scope

### In Scope

- Shared dashboards that all authorized users can create and edit, with changes saved automatically.
- A six-column desktop grid where new widgets default to 2 by 2 cells and cannot be smaller than 2 by 2; line charts, bar charts, tables, and text/notes can each be resized. Repositioning a widget moves other widgets out of the way.
- A builder-first slice with clearly identified unconfigured widgets, without invented live data. Define and preserve widget configuration for future source selection.
- A source-agnostic contract for retrieving approved graph data, with provider-specific configuration and a common graph-ready result.
- Selecting an available source per graph, selecting an approved dataset and mapping its fields to a compatible graph, and persisting the configuration.
- Managing approved source connections and access separately from configuring graphs.
- PostgreSQL as the first provider direction, with QuickBooks Online and Google Sheets delivered as separately scoped integrations. No initial live dataset is approved yet.
- Source and graph validation, authorization, refresh and failure states, accessible data equivalents, and operational guidance.
- A delivery path that permits PostgreSQL-backed graphs to work without waiting for either external integration.

### Out of Scope

- Arbitrary SQL or unrestricted access to application tables, spreadsheets, or QuickBooks data.
- Writing back to external sources, bulk replication of their entire contents, or using external data to change the Director's Report publication or meeting snapshots.
- A public connector marketplace, multi-tenancy, general-purpose analytics platform, or external-client API.
- Exports, scheduled report delivery, and personal dashboard layouts.
- Assuming specific business metrics, additional graph types, QuickBooks entities, spreadsheet layouts, or refresh intervals are approved.

## UX

### User Flows

1. An authorized user creates or opens a shared dashboard and adds a line chart, bar chart, table, or text/notes widget.
2. The user repositions or resizes widgets on the grid; other widgets move out of the way and changes save automatically. Save failures and concurrent-edit conflicts are reported rather than silently losing changes.
3. Before an approved live dataset is connected, data-driven widgets clearly indicate that they are unconfigured. Text/notes remain editable.

Later source-integration flows:

1. An authorized administrator makes a supported source available, supplies or authorizes its connection using the approved credential mechanism, and tests access without exposing secrets. PostgreSQL uses the existing application database configuration.
2. An authorized dashboard configurator adds or edits a graph, selects an available source and an approved dataset, and chooses compatible fields, metric meaning, units, aggregation, reporting period, and applicable filters.
3. The configurator previews the graph and its accessible data equivalent, resolves any validation errors, and saves the configuration. The graph retains its chosen source after reload.
4. An authorized viewer sees the graph with source attribution, reporting context, and freshness information, and can request a refresh.
5. If a source is disconnected, access is revoked, its schema changes, or retrieval fails, the graph reports the problem rather than silently switching sources or presenting unavailable data as zero. Other graphs remain usable.

All authorized users may create and edit shared dashboards. Connection management and access to source datasets remain separate permissions; editing a dashboard does not grant access to data or credentials. Provider connection authorization is separate from signing in to HELP-Board.

### Key Screens / Components

- Source management with provider type, connection status, test/reconnect actions, and non-sensitive connection details.
- Graph configuration with source and dataset selection, field mapping, validation, and preview.
- Dashboard graph with title, source, metric meaning, units, period, filters, freshness, and an accessible summary/data view.

### States

- No sources available, source not configured, connection pending, connected, disconnected, and access revoked or expired.
- No compatible datasets, invalid or changed field mapping, preview loading, empty data, save success, and save failure.
- Graph loading, empty, stale, partially available, unavailable, and failed; any retained last-successful data must be clearly identified as stale and subject to current authorization.

Saving an invalid or failed edit must not silently replace a previously valid configuration. A graph with no data must be distinguishable from a graph whose true value is zero.

### Builder implementation

The builder creates shared dashboards and saves committed field edits and widget
changes automatically. It uses move and resize buttons, including keyboard
operation, rather than drag-and-drop. New widgets occupy the first available
2-by-2 space. Moving or resizing places the changed widget first, then processes
the other widgets by row, column, and identifier, pushing collisions downward.
The grid has six desktop columns; narrow screens display widgets in a single
column without changing their saved positions.

Dashboard writes carry a revision checked atomically by the database. A failed
save leaves the local draft visible with a retry option; a conflict requires
explicitly discarding the draft and reloading. A failed create may have reached
the server even if its response was lost, so check the dashboard list before
retrying by reloading the page. A dashboard supports at most 50 widgets, 20 rows per widget, and
1,000 grid rows. Titles are limited to 100 characters and notes to 4,000.
These limits also apply server-side.

Line charts, bar charts, and tables display an unconfigured message; no source
selection or live data is implemented. Widget text and layout are stored as
dashboard configuration, not as credentials or source data.

Automated coverage includes layout collisions and bounds, authenticated and
antiforgery-protected API writes, invalid layouts, stale revisions, a SQLite
concurrency test, and component rendering. Browser smoke tests used mocked API
responses to exercise widget creation, keyboard movement and resizing, notes,
reload, conflicts, a narrow viewport, and all six themes. These tests are not a
full accessibility, production-authentication, or PostgreSQL migration review.
The migration was applied to a disposable PostgreSQL 17 database containing an
existing test ticket; the ticket remained intact. The local EF CLI warned that
its preview version is older than the 10.0.13 runtime. Production migration and
recovery planning are still outstanding.

## Data Needs

### Data Sources

| Source | Intended capability | Decisions before delivery |
|--------|---------------------|---------------------------|
| Application PostgreSQL | Read approved application-owned datasets through existing EF Core/repository boundaries; first provider. | Approved datasets and metric definitions, including which published report data may be exposed. |
| Google Sheets | Read an approved spreadsheet and worksheet/range, mapping columns to graph fields. | Account/authorization method, approved sheets and ranges, header and type rules, and refresh policy. |
| QuickBooks Online | Read approved company data through its API and map selected fields or report results to graph data. | Company connection and authorization, approved entities/reports and metrics, accounting periods, currency, and refresh policy. |

These are requested provider types, not approval of every dataset or credential scope. Google Workspace sign-in does not by itself grant Sheets access; QuickBooks authorization is independent. Exact provider APIs, supported data shapes, quotas, consent, and organizational approvals must be verified before each integration is implemented.

### Data Model Considerations

Conceptual records, not a prescribed schema:

- **Source connection:** stable identity, provider type, display name, status, non-secret settings, and a secure reference to credentials where required.
- **Approved dataset:** connection, permitted data selection, field names and types, and metric/aggregation constraints.
- **Dashboard and widget configuration:** shared dashboard identity, widget type and content, grid position and size, and revision/concurrency information; data-driven widgets later retain source and dataset references, mappings, metric/units, period, filters, and refresh policy.
- **Retrieval result:** typed graph-ready values and labels, completeness/error state, retrieval time, and source observation/update time when available.

Persist configuration in application PostgreSQL with reviewed EF Core migrations. Credentials and tokens must not be stored in graph configuration, returned to the browser, logged, or committed. Use an approved server-side secret storage and token lifecycle; document rotation, revocation, and recovery. Referential integrity and explicit behavior for source deletion, disconnection, and in-use configuration changes must be designed before implementation; never silently repoint graphs.

Use approved, bounded, server-side queries and aggregations. Validate mappings and types on the server, including missing columns, invalid values, dates, units, and duplicate or ambiguous headers. Define how nulls, missing periods, and actual zeros are represented. Historical charts require actual time-qualified observations or explicitly defined snapshots; a refresh must not manufacture history. Preserve source, period, currency, and aggregation context so unlike values are not silently combined.

### Data Freshness

- Define refresh triggers, cache lifetime, stale thresholds, and limits per source and dataset before release; do not assume a universal interval.
- Show the last successful retrieval time separately from the source's data-as-of time when available; if the latter is unknown, say so.
- Manual refresh requests a new retrieval but does not imply the underlying data changed. Failed refreshes must not mark old data as current.
- Bound result size, request duration, retry behavior, concurrent refreshes, and cache use for provider quotas and PiTony/PiDB capacity. Cache keys and access checks must respect source, graph, filters, and viewer permissions.

## Accessibility (WCAG)

- Moving and resizing widgets must have keyboard-operable alternatives, visible focus, and clear announcements; do not rely on dragging alone.
- Source and field selectors, connection actions, preview, and refresh must be keyboard operable with associated labels, instructions, and errors.
- Announce connection, loading, save, and refresh outcomes without relying on color or stealing focus.
- Provide each graph with a meaningful text summary and accessible data equivalent; do not rely on color, hover, or chart graphics alone.
- Validate focus order, responsive layouts, zoom, and contrast across the standard themes.

## Security and Operations

- Enforce authentication and source-management, graph-configuration, and graph-view permissions on the API, not only in the UI. A configurator's ability to select a source must not grant viewers unauthorized underlying data.
- Restrict source and dataset access to approved data and least-privilege provider credentials. Keep provider calls, OAuth exchanges and tokens server-side; protect callbacks and use the approved public HTTPS origin.
- Handle expiration, revocation, provider errors, rate limits, and reconnect explicitly without logging secrets or sensitive source values. Failed access checks must not serve cached data.
- Document connection setup, required provider permissions, credential rotation/revocation, source health, troubleshooting, and deployment/recovery for each released provider.

## Dependencies

- Feature 002 dashboard foundation; shared dashboard creation and grid editing are implemented here.
- Feature 005 authentication and approved authorization policies for source management, configuration, and viewing.
- Feature 009 PostgreSQL/EF Core, reviewed migrations, and the existing PiDB topology.
- Feature 010 is on hold and is not required for the builder. If its data is used later, respect publication and access rules and never expose drafts to board viewers.
- Features 001 and 008 for accessible UI and standard themes; Feature 007 for the agreed rollout/flag strategy; Feature 006 for deployment and secret handling.
- Approved datasets, metric definitions, provider accounts, access grants, and operational capacity for each delivery slice.

## Risks

- Misconfigured permissions or caches could expose sensitive financial or unpublished report data.
- Provider outages, quotas, expired credentials, and changing spreadsheet columns or API schemas can invalidate graphs.
- Incorrect date, currency, aggregation, or missing-value interpretation can make charts misleading.
- Unbounded external calls or database queries can exhaust shared PiTony/PiDB resources.
- Source removal or credential loss can break existing graphs unless dependencies and recovery are explicit.

## Copilot Governance

### Candidate Instructions

- Require source-specific authorization, credential handling, data semantics, freshness, and failure-state review for every connector.
- Require graph changes to preserve source attribution, accessible data equivalents, and distinction between missing, stale, and zero values.

### Candidate Skills

- Evaluate a reusable connector security and data-contract review workflow after the first provider; no new skill is required solely by this draft.

### Reusable Prompts

- "Review this connector for least-privilege access, credential exposure, refresh failures, and revoked permissions."
- "Trace each graph value to its source, reporting period, unit, aggregation, and last successful retrieval."

## Acceptance Criteria

- [ ] An authorized user creates a shared dashboard and another authorized user can view and edit it; anonymous access is denied.
- [x] Users add line chart, bar chart, table, and text/notes widgets. Each starts at 2 by 2 grid cells, supports resizing, and cannot be smaller than 2 by 2. The desktop grid has six columns.
- [x] Users reposition widgets on the grid; colliding widgets move out of the way without overlap. Keyboard users can move and resize widgets too.
- [ ] Dashboard and widget changes save automatically and survive reload; failed saves and concurrent edits are surfaced without silent data loss.
- [x] Before live data is available, data-driven widgets clearly show an unconfigured state rather than invented financial values.
- [x] The feature flag is enabled in the current web configuration after the builder is ready; missing or disabled flags retain the existing experience.
- [ ] An authorized configurator selects an approved source and dataset for each graph, maps compatible fields, previews and saves it; a reload retains each graph's selection.
- [ ] PostgreSQL is the first working provider, using approved application-owned data and existing persistence boundaries. Its delivery does not require Google Sheets or QuickBooks Online.
- [ ] Google Sheets and QuickBooks Online are delivered as subsequent provider slices, each with approved account access, dataset/metric definitions, connection and recovery guidance, and the same graph contract and authorization checks; their order is not yet fixed.
- [ ] Different graphs can use different approved sources; an unavailable source does not prevent independent graphs from rendering or silently change their sources.
- [ ] Server-side validation rejects incompatible mappings and unauthorized source/dataset access; failed edits retain the last valid configuration.
- [ ] Tests cover empty data, actual zero, missing/invalid fields, source schema changes, provider errors, revoked access, failed refresh, and permission-safe caching.
- [ ] Graphs display source attribution, metric meaning, units, period, filters, last successful retrieval, and source data time when available. Stale or partial results are labeled; unavailable results are not shown as zero.
- [ ] Accessible keyboard flows, error/status announcements, graph text summaries and data equivalents pass the project's accessibility and theme checks.
- [ ] Secrets and tokens remain server-side and out of logs, source control, and browser assets; connection, refresh, rate-limit, rotation, and recovery procedures are documented.
- [ ] Approved limits for query size, timeout, refresh concurrency, retries, and cache use are tested against the intended deployment and provider constraints.
- [ ] Required governance updates and the feature rollout/flag strategy are documented.

## Open Questions

- Who may manage connections, configure graphs, approve datasets, and view each dataset? Is a separate review or publication step required for dashboard changes?
- Which application datasets, spreadsheet ranges, QuickBooks company/entities or reports, and specific metrics are approved first?
- What graph types, field mappings, filters, date/aggregation rules, and currency handling are needed for the initial graphs?
- Which organization-owned accounts and authorization methods should be used for Sheets and QuickBooks, and where should credentials be stored and recovered?
- What refresh intervals, stale thresholds, cache retention, rate limits, and resource budgets are acceptable for each provider?
- What happens to graphs when a source is disabled, deleted, reauthorized, or changes schema? May authorized users see retained stale values, and for how long?
- What is the intended delivery order and release timing for Google Sheets and QuickBooks Online after PostgreSQL?
- What widget maximum sizes and deterministic collision-placement rules best fit desktop and mobile layouts?
