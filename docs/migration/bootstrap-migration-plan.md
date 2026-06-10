# Bootstrap Migration Plan (Feature 001)

Operational plan to retire Bootstrap-dependent UI patterns and move all foundational surfaces to the in-repo design system.

## Current State

- Phase 0 is complete: foundation is ready (tokenized styles, app shell baseline, icon wrapper, accessibility-first focus strategy).

## Phase Plan

| Phase ID | Phase Name | Owner Role | Entry Criteria | Exit Criteria | Validation | Rollback Notes |
|---|---|---|---|---|---|---|
| P0 | Foundation Ready (Complete) | Frontend Lead | Feature 001 foundation approved | Tokens, shell primitives, icon wrapper, and accessibility baseline are in place | Visual smoke checks + keyboard pass on shell/dashboard | Keep current foundation branch/tag as rollback anchor |
| P1 | Inventory and Mapping | UI Engineer | P0 complete; target surfaces identified | Bootstrap class inventory exists with replacement mapping per surface | Spot-check mapped pages for missing equivalent states | If mapping is incomplete, do not start replacement; keep Bootstrap usage unchanged |
| P2 | Layout and Navigation Replacement | UI Engineer | Mapping for shell/nav components approved | Header/nav/layout surfaces no longer depend on Bootstrap classes | Keyboard pass, skip-link check, route-focus behavior check | Revert affected layout PR(s); keep old nav/layout styles available until re-validated |
| P3 | Core Controls and Feedback States | UI Engineer | P2 complete; control specs approved | Buttons, inputs, alerts, badges, and table wrappers use design-system styles | Manual interaction pass + axe/WAVE on changed pages | Revert control-specific PR(s); preserve token files and only roll back component styles |
| P4 | Bootstrap Asset Removal and Cleanup | Frontend Lead | P3 complete; no required Bootstrap selectors remain | Bootstrap assets/references removed and app remains stable | Full regression smoke on key routes + accessibility checklist | Restore prior asset references in one rollback commit if regressions appear |

## Operating Notes by Phase

### Planning and Ownership

- One phase per PR series; do not mix phases in one large PR.
- Owner role is accountable for checklist completion and evidence links.

### Validation Expectations

- Required: keyboard-only pass, screen reader smoke check, axe, WAVE.
- Required: visual check for spacing/typography/status consistency against tokens.

### Rollback Approach

- Keep rollback small and phase-local.
- Prefer reverting phase PR(s) rather than introducing emergency patch layers.

## PR Tracking Template (Per Phase)

Use one row per PR tied to a migration phase.

| Phase ID | PR # / Link | Owner | Scope Summary | Validation Evidence | Rollback Plan | Status |
|---|---|---|---|---|---|---|
| P1 | TBD | TBD | Bootstrap inventory and replacement mapping | Keyboard pass N/A, axe/WAVE N/A, peer review link | Revert PR if mapping misses critical surfaces | Planned |
| P2 | TBD | TBD | Layout/nav replacement | Keyboard pass link, skip-link check, focus-on-route check | Revert layout PR set | Planned |
| P3 | TBD | TBD | Controls/feedback replacement | Keyboard pass link, screen reader notes, axe/WAVE reports | Revert control PR set | Planned |
| P4 | TBD | TBD | Remove Bootstrap assets and cleanup | Route smoke results + accessibility checklist | Restore asset refs in rollback commit | Planned |
