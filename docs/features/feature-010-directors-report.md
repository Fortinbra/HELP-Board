# Feature 010: Director's Report

Status: Requirements draft; not implemented  
Last updated: 2026-10-16

## Goal

Deliver the first HELP-Board business report: a director-completed form that generates an interactive report for authorized board members, remains current between meetings, and preserves meeting records and historical trends.

This feature implements [global requirements GLB-014 and DIR-001 through DIR-012](../global-requirements.md#51-first-report-directors-report). Report sections, fields, metric definitions, and chart choices are undecided.

## User Value

The director can prepare and update one authoritative report per meeting without exposing unfinished edits. Board members can read the latest publication, review what was presented at previous meetings, and inspect comparable trends without losing historical context.

## Scope

### In Scope

- Director-facing form with persisted drafts, validation, and explicit publication.
- One logical report per board meeting, with 12 regular meetings per year and support for additional ad hoc meetings.
- Continued updates to the upcoming meeting's live report between meetings.
- Published reports available to all authorized board members, not the public.
- Fixed meeting snapshots with subsequent corrections stored separately.
- Meeting archive, source-value inspection, and trends for approved comparable metrics.
- Database-backed history, author attribution, publication metadata, permissions, and accessible report/form states.

### Out of Scope

- Downloads, PDF generation, print-specific layouts, or scheduled report delivery.
- Public report links or anonymous access.
- Assuming particular financial, personnel, program, participant, or other business fields before approval.
- Treating ticket counts as Director's Report data.
- A generalized report/form builder, meeting minutes system, agenda/voting platform, or automatic board calendar integration.
- External-client API versions or alternative production deployments.

## UX

### User Flows

1. An authorized user identifies or creates the target meeting/report using a meeting-management policy still to be agreed.
2. The director completes approved narrative and structured fields and saves a draft.
3. The director reviews the generated representation and explicitly publishes it.
4. Board members see the latest successful publication, with meeting and reporting-period context.
5. Between meetings, the director edits a new draft and republishes when ready. The existing publication remains available until publication succeeds.
6. At the agreed meeting-record event, an authorized action preserves a fixed snapshot of the publication considered by the board. A later unpublished draft is not substituted.
7. A later correction is recorded and linked separately, leaving the original snapshot available and clearly distinguishable from amendments.
8. Board members select a prior meeting or inspect comparable metrics over time.

### Key Screens / Components

- Meeting/report selector distinguishing regular and ad hoc meetings.
- Director form with save, validation feedback, and publication review.
- Read-only published report with narrative, approved values, appropriate charts, and accessible data equivalents.
- Clear draft/published/as-of metadata appropriate to the viewer's permissions.
- Meeting history with fixed snapshot and linked correction views.
- Trend view with explicit periods, source revision/snapshot context, and missing-data handling.

### States

- No report yet, draft only, published, and published with newer unpublished draft.
- Saving/publishing, saved/published confirmation, validation failure, and failed operation with retry guidance.
- Snapshot not yet fixed, snapshot preserved, and linked correction available.
- No historical data, insufficient comparable points, missing metric, and unavailable trend.
- Read-only/unauthorized, stale view, and concurrent-edit conflict.

Draft content is visible only to authorized authors. Viewers must never confuse an unpublished report with a valid zero/empty completed report. An archived meeting record must not silently become the latest live report.

## Data Needs

### Data Sources

- Director-entered form data persisted in application-owned PostgreSQL through EF Core.
- Approved meeting metadata and report/metric definitions.
- Retained publications, fixed meeting snapshots, and linked corrections.

### Data Model Considerations

These are conceptual records, not a prescribed table design:

- **Meeting:** stable identity, regular/ad hoc type, scheduled date, and agreed lifecycle metadata. Identity must survive rescheduling and must not be keyed solely by month/date.
- **Logical report:** stable association with one meeting; enforce one logical Director's Report per meeting server-side and in persistent integrity rules.
- **Draft:** editable inputs and attribution, distinct from what board members currently see.
- **Publication:** attributable persisted revision containing a consistent view of approved inputs, reporting-period/as-of context, and publication time.
- **Meeting snapshot:** fixed reference to or preserved copy of the relevant publication, with recorded freeze metadata.
- **Correction:** separately attributable amendment linked to its meeting snapshot; approval/publication rules remain open.
- **Metric/field context:** retain definitions, units, and reporting-period semantics needed to interpret old values when the form evolves.

Save and publish operations must avoid duplicate reports, partial publications, lost updates, and accidental historical overwrite. The concurrency strategy must be agreed and tested before implementation acceptance.

Twelve regular meetings is the expected annual schedule, not a report-count limit or proof that each report measures a separate month. Support multiple meetings in one month, ad hoc meetings, and rescheduling without replacing an earlier meeting's report.

### Data Freshness and History

- Draft saves persist working content without changing the published report.
- Successful publication makes that revision available to authorized readers; define cache invalidation and refresh behavior without assuming push infrastructure.
- Preserve the distinction between observation/reporting period, meeting date, save time, publication time, and snapshot/correction time.
- Never copy current values into past periods to manufacture a trend.
- Define metric-specific comparison rules before choosing charts: units, period boundaries, aggregation grain, missing values, inclusion of ad hoc meetings, and correction treatment.
- Overlapping measurement periods must not be double-counted in totals. A report revision is not automatically a new independent data observation.
- Trend views must identify whether they use original meeting snapshots or corrected values and retain a path to the relevant source records.
- Carry-forward, automatic next-report creation, retention duration, cancellation, and snapshot timing remain explicit open decisions.

## Accessibility (WCAG)

- Associate labels, help text, required state, and errors with form controls; provide a keyboard-reachable error summary.
- Ensure saving/publication status announcements do not steal focus or imply success before persistence completes.
- Support keyboard-only report creation, review, publication, history selection, and chart inspection.
- Provide text summaries and accessible data views for charts, with no color-only or hover-only meaning.
- Validate Light, Dark, and HELP themes, focus behavior, and supported responsive/zoom layouts.

## Dependencies

- Feature 001 shared UI and Feature 008 standard themes.
- Feature 002 dashboard foundation; this feature supplies its first business-data reporting slice rather than expanding that foundation's historical scope.
- Feature 005 identity and distinct author/view permissions.
- Feature 009 PostgreSQL/EF Core and reviewed migrations.
- Feature 007 flag strategy with explicit enablement approval.
- Feature 006 deployment and recovery on PiTony.
- Approved form/metric definitions and meeting/publication/correction policies.

## Risks

- Incorrectly treating monthly meeting frequency as measurement grain can double-count data.
- Updating a shared mutable record can destroy what the board actually saw.
- Publishing drafts accidentally or linking corrections without access checks can expose unfinished/restricted information.
- Carry-forward can present stale values as newly confirmed observations.
- Undefined field semantics or changed units can make historic comparisons misleading.
- Concurrent saves/publications can lose edits or associate the wrong revision with a snapshot.

## Copilot Governance

### Candidate Instructions

- Preserve draft/publication/snapshot separation, access boundaries, and immutable meeting records in report changes.
- Require metric-definition and trend-comparability notes when fields, aggregation, or correction rules change.

### Candidate Skills

- Evaluate whether a reusable report-lifecycle validation workflow is warranted after this first implementation; no new customization is required solely because it is suggested here.

### Reusable Prompts

- "Verify that saving a draft cannot change the report visible to board members or a preserved meeting snapshot."
- "Test 12 regular meetings plus ad hoc meetings with overlapping reporting periods; prove trends do not double-count."
- "Show which publication, metric definition, reporting period, and corrections explain each historical chart point."

## Acceptance Criteria

- [ ] Approved form sections, validation rules, and chart selections are documented before implementation acceptance.
- [ ] Authorized director saves a draft in PostgreSQL and resumes it without exposing draft inputs to board viewers.
- [ ] Explicit publication generates a consistent interactive report visible to all authorized board members; anonymous visitors and unauthorized authors are denied by the API.
- [ ] Republishing between meetings changes the visible live report only after success; failed saves/publications retain the previous publication.
- [ ] Twelve regular meetings plus at least two ad hoc meetings in one year retain distinct reports, including multiple meetings in one month.
- [ ] Rescheduling preserves meeting/report identity; attempts to create a duplicate logical report are rejected explicitly.
- [ ] The agreed freeze action preserves exactly the intended published revision, never a newer unpublished draft.
- [ ] Later updates and separately published corrections do not overwrite the fixed meeting snapshot.
- [ ] Meeting archive and trend views trace values to their periods, definitions, and relevant publications/snapshots/corrections.
- [ ] Known-value tests cover missing values, actual zero, overlapping periods, ad hoc inclusion, corrections, and changed field definitions without fabricated or double-counted history.
- [ ] Concurrency tests prove saves, publication, and snapshot operations cannot silently lose edits or mix revisions.
- [ ] Form/report accessibility, theme parity, refresh states, and approved PiTony performance checks pass.
- [ ] Flag ownership/defaults/approval, lifecycle verification, and any warranted governance updates are documented.

## Open Questions

- What sections and fields does the director enter, and which are required? Who approves validation, metric semantics, and chart types?
- Who may create/reschedule meetings and author/publish on the director's behalf, if anyone?
- What exact action/event freezes the report, who performs it, and what happens if no publication exists at that time?
- How are corrections submitted, approved, published, and reflected in trends while preserving the original?
- What periods do metrics measure, and how do ad hoc meetings participate in comparison views?
- How are canceled meetings and multiple upcoming meetings handled?
- Should the next report be created manually or automatically? Is carry-forward allowed, and how is it marked unconfirmed?
- What concurrency, retention, and post-publication refresh policies apply?
