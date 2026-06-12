# Feature 003: Strategic Plan Tracker

## Goal
Define how the Strategic Plan is represented as the always-present required system project board, with reserved identity/name and non-deletable constraints, using the shared board/work-item model so leadership and teams can continuously assess execution progress.

## User Value
Users gain transparency into strategic progress, can detect stalled initiatives early, and can connect day-to-day execution to strategic outcomes.

## Scope
### In Scope
- Define Strategic Plan as a required system project board that always exists, with reserved identity/name and non-deletable constraints.
- Define strategic item taxonomy (themes/objectives/initiatives/milestones) as typed work items within that system board.
- Define ownership, status, and progress roll-up behavior for strategic items on the system board.
- Define reporting views and dashboard integration points using the shared board model.

### Out of Scope
- Full enterprise portfolio management features.
- Advanced forecasting and predictive analytics.

## UX
### User Flows
- User opens the Strategic Plan system board from the board directory.
- User views strategic plan overview on that board.
- User opens an objective item to inspect linked initiative and milestone items.
- Owner updates item status and progress.
- Leader reviews cross-objective health summary.

### Key Screens / Components
- Strategic overview board.
- Objective detail view with linked initiatives.
- Milestone timeline/checkpoint view.
- Ownership and accountability panel.

### States
- Planned, on track, at risk, blocked, completed, archived.

## Data Needs
### Data Sources
- Strategic Plan board metadata (system board identity).
- Strategic objective/initiative/milestone item records stored on that board.
- Linked work from other project boards.

### Data Model Considerations
- Shared board and work-item model applies to both project boards and the Strategic Plan system board.
- Strategic hierarchy uses explicit parent/child IDs across strategic item types within the same shared work-item model.
- No separate strategic-only persistence model is introduced in this feature documentation.
- Progress roll-up is bottom-up across the hierarchy: milestone values roll into initiatives, and initiative values roll into objectives; if a child set does not specify weighting, children are treated as equally weighted for the parent roll-up.
- Current groundwork requires each strategic work item to include owner attribution plus latest update metadata (`updatedBy` and `updatedAt`) alongside status and progress; historical audit trail support is not yet part of this feature.

### Data Freshness
- Near-real-time updates on manual status changes.
- Historical snapshots for trend reporting.

## Accessibility (WCAG)
- Hierarchical views must be navigable by keyboard and screen reader.
- Status indicators require text labels alongside visual cues.
- Timelines and progress visuals need alternative text summaries.
- Focus management is required for expand/collapse interactions.

## Dashboard Integration Points
- Strategic health summary widget.
- Objectives at risk widget.
- Milestone due-soon indicator.
- Drill-down from dashboard card to Strategic Plan system board and objective detail page.

## Dependencies
- Feature 001 UI Foundation & Theme System.
- Feature 002 Dashboard Experience for visibility integration.
- Feature 004 Project Boards for shared board model and board identity constraints.
- Data contracts for strategic entities and status roll-up logic.

## Risks
- Ambiguous roll-up rules may cause distrust in reported progress.
- Too much manual data entry can reduce update consistency.
- Complex hierarchy visualization may hurt usability without careful design.

## Copilot Governance
### Candidate Instructions
- Require every change to strategic entities to preserve explicit hierarchy rules, owner attribution, and update history expectations.
- Require roll-up logic changes to describe how milestone, initiative, and objective health calculations are derived and tested.
- Require accessibility notes for hierarchy navigation, expand/collapse behavior, and timeline summaries in every related UI change.

### Candidate Skills
- Strategic model review skill to verify hierarchy integrity, roll-up consistency, and linkage to project boards.
- Strategic accessibility/reporting skill to review timeline, hierarchy, and dashboard-summary behavior.

### Reusable Prompts
- "Review this strategic tracker change for roll-up correctness, ownership accountability, and hierarchy clarity."
- "List the tests and documentation needed when strategic status or progress roll-up rules change."
- "Identify whether this strategic feature change introduces new governance rules that should become Copilot instructions."

## Acceptance Criteria
- [x] Documentation explicitly states that Strategic Plan is the required system project board that always exists, with reserved identity/name and non-deletable constraints.
- [x] Documentation defines strategic objectives, initiatives, and milestones as typed strategic work items within that system board.
- [x] Documentation defines integration with other project boards through the shared board/work-item model with Strategic Plan constraints and identity.
- [x] Documentation defines roll-up logic for progress and health, including the milestone -> initiative -> objective path and the default equal-weighting assumption when no explicit weighting is provided.
- [x] Documentation defines ownership and update accountability requirements, including the required owner plus latest update metadata fields for current groundwork.
- [x] Documentation explicitly lists dashboard integration points.
- [x] Documentation defines accessibility expectations for hierarchy and timeline views.
- [x] Documentation defines governance artifacts for strategic planning work, covering roll-up logic, accountability, and accessibility guardrails.

Status: Complete (validated 2026-06-11).

## Open Questions
- How frequently should plan health require manual confirmation by owners?
