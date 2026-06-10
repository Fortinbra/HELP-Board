---
applyTo: "src/HelpBoard.Web/**/*.{razor,razor.css,css}"
description: "Use when building or migrating UI surfaces to prevent new Bootstrap class usage while allowing controlled, temporary migration exceptions."
---

# Bootstrap Migration Governance

## Use When

Use when creating new UI surfaces or migrating existing Razor/CSS UI from Bootstrap to the HelpBoard design system.

## Rules

- Do not introduce Bootstrap utility, layout, or component classes in new UI surfaces.
- Prefer HelpBoard-owned classes, tokens, and component patterns for all new work.
- Existing Bootstrap usage may remain only during active migration and must be reduced over time.
- A temporary Bootstrap exception is allowed only with an explicit inline comment and linked tracking issue.
- Exception comments must include a removal intent and issue reference, for example: `<!-- Bootstrap migration exception: remove by #123 -->`.

## Enforcement Expectations

- PRs adding new Bootstrap classes to non-migrating surfaces should be blocked.
- Migration exceptions should be narrowly scoped and removed as soon as the replacement is available.
