# Feature 011: Dashboard Data Sources

Status: Requirements draft; not implemented
Last updated: 2026-11-18

## Goal

Allow an authorized dashboard configurator to select a data source for each graph component and configure the data it visualizes. Introduce the application-owned PostgreSQL database first, followed by Google Sheets and QuickBooks Online integrations.

This feature refines [GLB-004 and the reporting and integration requirements](../global-requirements.md#4-architecture-and-persistence). The [dashboard requirements](../global-requirements.md#5-dashboards-and-interactive-reporting) include source-agnostic guidance; graph configuration and data-source selection are not yet implemented. This is a purpose-built HELP-Board feature, not a general-purpose dashboard product.

## User Value

Dashboard configurators can choose an appropriate approved source for each graph instead of relying on a single hard-coded source. Board members can see meaningful, attributable visualizations while understanding when data is missing, stale, or unavailable.

## Scope

### In Scope

- A source-agnostic contract for retrieving approved graph data, with provider-specific configuration and a common graph-ready result.
- Selecting an available source per graph, selecting an approved dataset and mapping its fields to a compatible graph, and persisting the configuration.
- Managing approved source connections and access separately from configuring graphs.
- Application-owned PostgreSQL as the first source, followed by Google Sheets and QuickBooks Online as separately delivered integrations.
- Source and graph validation, authorization, refresh and failure states, accessible data equivalents, and operational guidance.
- A delivery path that permits PostgreSQL-backed graphs to work without waiting for either external integration.

### Out of Scope

- Arbitrary SQL or unrestricted access to application tables, spreadsheets, or QuickBooks data.
- Writing back to external sources, bulk replication of their entire contents, or using external data to change the Director's Report publication or meeting snapshots.
- A public connector marketplace, multi-tenancy, general-purpose analytics platform, or external-client API.
- Exports, scheduled report delivery, personal dashboard layouts, and self-service report building beyond the authorized graph configuration described here.
- Assuming specific business metrics, graph types, QuickBooks entities, spreadsheet layouts, or refresh intervals are approved.

## UX

### User Flows

1. An authorized administrator makes a supported source available, supplies or authorizes its connection using the approved credential mechanism, and tests access without exposing secrets. PostgreSQL uses the existing application database configuration.
2. An authorized dashboard configurator adds or edits a graph, selects an available source and an approved dataset, and chooses compatible fields, metric meaning, units, aggregation, reporting period, and applicable filters.
3. The configurator previews the graph and its accessible data equivalent, resolves any validation errors, and saves the configuration. The graph retains its chosen source after reload.
4. An authorized viewer sees the graph with source attribution, reporting context, and freshness information, and can request a refresh.
5. If a source is disconnected, access is revoked, its schema changes, or retrieval fails, the graph reports the problem rather than silently switching sources or presenting unavailable data as zero. Other graphs remain usable.

Connection management, graph configuration, and viewing are distinct permissions; their exact role assignments and configuration/publishing workflow remain open. Provider connection authorization is separate from signing in to HELP-Board.

### Key Screens / Components

- Source management with provider type, connection status, test/reconnect actions, and non-sensitive connection details.
- Graph configuration with source and dataset selection, field mapping, validation, and preview.
- Dashboard graph with title, source, metric meaning, units, period, filters, freshness, and an accessible summary/data view.

### States

- No sources available, source not configured, connection pending, connected, disconnected, and access revoked or expired.
- No compatible datasets, invalid or changed field mapping, preview loading, empty data, save success, and save failure.
- Graph loading, empty, stale, partially available, unavailable, and failed; any retained last-successful data must be clearly identified as stale and subject to current authorization.

Saving an invalid or failed edit must not silently replace a previously valid configuration. A graph with no data must be distinguishable from a graph whose true value is zero.

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
- **Graph configuration:** stable graph identity, source and dataset references, visualization type, field mappings, metric/units, period, filters, and refresh policy.
- **Retrieval result:** typed graph-ready values and labels, completeness/error state, retrieval time, and source observation/update time when available.

Persist configuration in application PostgreSQL with reviewed EF Core migrations. Credentials and tokens must not be stored in graph configuration, returned to the browser, logged, or committed. Use an approved server-side secret storage and token lifecycle; document rotation, revocation, and recovery. Referential integrity and explicit behavior for source deletion, disconnection, and in-use configuration changes must be designed before implementation; never silently repoint graphs.

Use approved, bounded, server-side queries and aggregations. Validate mappings and types on the server, including missing columns, invalid values, dates, units, and duplicate or ambiguous headers. Define how nulls, missing periods, and actual zeros are represented. Historical charts require actual time-qualified observations or explicitly defined snapshots; a refresh must not manufacture history. Preserve source, period, currency, and aggregation context so unlike values are not silently combined.

### Data Freshness

- Define refresh triggers, cache lifetime, stale thresholds, and limits per source and dataset before release; do not assume a universal interval.
- Show the last successful retrieval time separately from the source's data-as-of time when available; if the latter is unknown, say so.
- Manual refresh requests a new retrieval but does not imply the underlying data changed. Failed refreshes must not mark old data as current.
- Bound result size, request duration, retry behavior, concurrent refreshes, and cache use for provider quotas and PiTony/PiDB capacity. Cache keys and access checks must respect source, graph, filters, and viewer permissions.

## Accessibility (WCAG)

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

- Feature 002 dashboard foundation and an authorized graph configuration workflow, neither of which currently provides this feature.
- Feature 005 authentication and approved authorization policies for source management, configuration, and viewing.
- Feature 009 PostgreSQL/EF Core, reviewed migrations, and the existing PiDB topology.
- Feature 010 publication and access rules when Director's Report data is selected; never expose drafts to board viewers.
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

- [ ] An authorized configurator selects an approved source and dataset for each graph, maps compatible fields, previews and saves it; a reload retains each graph's selection.
- [ ] PostgreSQL is the first working provider, using approved application-owned data and existing persistence boundaries. Its delivery does not require Google Sheets or QuickBooks Online.
- [ ] Google Sheets and QuickBooks Online are delivered as subsequent provider slices, each with approved account access, dataset/metric definitions, connection and recovery guidance, and the same graph contract and authorization checks.
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
