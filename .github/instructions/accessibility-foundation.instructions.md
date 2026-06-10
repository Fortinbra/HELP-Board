---
applyTo: "src/HelpBoard.Web/**/*.{razor,razor.css,css}"
description: "Use when creating or updating UI markup and styles to enforce baseline accessibility requirements and WCAG AA readiness."
---

# Accessibility Foundation Governance

## Use When

Use when implementing or modifying UI markup, interaction flows, and styling in `HelpBoard.Web`.

## Rules

- All interactive controls must be keyboard reachable and operable without pointer input.
- Provide a visible focus indicator for focused interactive elements; do not remove focus styles without an accessible replacement.
- Use semantic landmarks (`header`, `nav`, `main`, `footer`, `aside`) where appropriate.
- Keep heading order logical and hierarchical; do not skip levels for visual styling convenience.
- Do not communicate status by color alone. Include text labels, icon + text, patterns, or other non-color cues.
- Validate color contrast and interaction patterns against WCAG 2.1 AA expectations.

## Enforcement Expectations

- Accessibility is a baseline quality gate, not a post-release enhancement.
- New components should be reviewed for keyboard flow, focus visibility, semantics, and contrast before merge.
