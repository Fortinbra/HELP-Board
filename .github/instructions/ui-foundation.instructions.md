---
applyTo: "src/HelpBoard.Web/**/*.{razor,razor.css,css}"
description: "Use when creating or updating shared UI surfaces in HelpBoard.Web to enforce design token usage, semantic class naming, and component consistency."
---

# UI Foundation Governance

## Use When

Use when building or modifying shared UI structure, reusable components, page shells, or foundational styles in `HelpBoard.Web`.

## Rules

- Use design tokens (CSS custom properties) for color, spacing, radii, shadows, and typography in shared UI.
- Do not hard-code raw values for shared UI styling (for example: `#fff`, `rgb(...)`, `16px`, `1rem`) unless the value is tokenized first.
- Name classes semantically by purpose, not by visual appearance. Prefer names like `ticket-card__meta` over `blue-box` or `left-col`.
- Keep component variants and states consistent across surfaces. Reuse established component patterns before introducing a new one.
- New shared UI primitives must expose stable, intention-revealing class hooks and avoid page-specific coupling.

## Enforcement Expectations

- If a new style value appears more than once, promote it to a token.
- Prefer extending existing component styles over duplicating near-identical markup and CSS.
- Treat this file as a guardrail for consistency, maintainability, and predictable theming.
