---
applyTo: "src/HelpBoard.Web/**/*.{razor,razor.css,css}"
description: "Use when implementing strategic hierarchy and timeline UI to enforce keyboard navigation, expand/collapse focus behavior, and timeline text summaries."
---

# Strategic Accessibility Governance

## Use When

Use when building or modifying strategic hierarchy, objective detail, initiative tree, or milestone timeline UI in `HelpBoard.Web`.

## Rules

- Hierarchy views must support keyboard navigation with predictable order and visible focus.
- Expand/collapse interactions must keep logical focus behavior (focus remains stable or moves to the revealed region heading/control).
- Timeline and progress visuals must provide text summaries that communicate the same status/progression information.

## Enforcement Expectations

- Validate keyboard-only traversal for hierarchy and expand/collapse controls before merge.
- Validate timeline summaries are available to assistive technologies and not color-only.
