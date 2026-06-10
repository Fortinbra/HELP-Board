---
applyTo: "src/HelpBoard.Web/**/*.razor"
description: "Use when rendering icons in Razor components to enforce AppIcon wrapper usage and accessible icon labeling rules."
---

# Icon Wrapper Governance

## Use When

Use when adding or changing icons in Razor components, including navigation, buttons, badges, status indicators, and empty states.

## Rules

- Render icons through the `AppIcon` component wrapper only.
- Do not use raw icon markup directly in feature components.
- Decorative icons must be hidden from assistive technology (`aria-hidden=\"true\"`) and must not expose redundant text.
- Semantic icons must include an accessible name via `aria-label` or equivalent nearby descriptive text.
- If an icon conveys status or meaning, pair it with text so meaning is not icon-only.

## Enforcement Expectations

- Prefer a single icon contract via `AppIcon` to keep rendering and accessibility behavior centralized.
- Any exception requires a documented reason in the PR description and a follow-up issue.
