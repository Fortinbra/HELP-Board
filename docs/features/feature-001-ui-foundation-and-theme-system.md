# Feature 001: UI Foundation & Theme System

## Goal

Establish a reusable, accessible UI foundation for HELP-Board with a teal/purple design language, an in-repo Blazor design system and component library, and consistent Lucide-aligned iconography.

## User Value

Users get a clear, modern, and accessible interface that makes work status easy to understand at a glance. Teams gain a stable visual system that reduces inconsistency as new features are added.

## Scope

### In Scope

- Define a design token system for color, spacing, typography, elevation, and interactive states.
- Build the initial in-repo Blazor design system foundation and shared component library for layout and foundational controls.
- Standardize icon usage with the Lucide icon set.
- Implement WCAG-focused accessibility baseline requirements for all new UI components.
- Provide an incremental migration path away from Bootstrap starter styles/components currently present in the app.

### Out of Scope

- Full implementation of dashboard, strategic plan tracker, or project board business logic.
- Finalized brand assets beyond the defined core palette and type scale.

## UX

### User Flows

- User logs in and lands on a dashboard shell with consistent global layout.
- User navigates sections using a keyboard-accessible menu and recognizable icons.
- User interprets status indicators using accessible color and text combinations.

### Key Screens / Components

- App shell (header, navigation, content region).
- Standard layout primitives (cards, panels, page headers, section containers).
- Core controls (buttons, links, inputs, toggles, tabs, alerts).
- Status badges and progress indicators.
- Icon wrapper component for Lucide icons with consistent sizing and labels.

### States

- Loading, empty, error, disabled, hover, active, selected, and focus-visible states for all core controls.

## Data Needs

### Data Sources

- None required for initial theme/system setup.

### Data Model Considerations

- Theme tokens should be represented as centrally managed CSS variables and component-level semantic aliases.

### Data Freshness

- Static at runtime for the default teal/purple release, with room for future user-selectable themes in Feature 008.

## Accessibility (WCAG)

- Meet WCAG 2.2 AA color contrast targets for text, icons, controls, and chart-adjacent UI.
- Ensure visible and consistent focus indicators for all interactive elements.
- Provide full keyboard navigation for global navigation and all core components.
- Use semantic HTML landmarks and heading hierarchy across layouts.
- Ensure icon-only actions include accessible names.
- Do not rely on color alone to communicate status.

## Framework and Library Decisions

- HELP-Board will use an in-repo Blazor design system and shared component library rather than adopt a third-party UI framework.
- Bootstrap is limited to legacy migration surfaces and must not be used for new foundational components.
- Lucide is the standard icon set for navigation, actions, and status-adjacent iconography, implemented through an internal icon wrapper for consistent sizing, labeling, and usage.

## Design Tokens (Initial)

### Primary Palette

- Teal Primary: `#0F766E`
- Teal Surface Accent: `#14B8A6`
- Purple Primary: `#6D28D9`
- Purple Surface Accent: `#A78BFA`
- Neutral Text: `#111827`
- Neutral Background: `#F8FAFC`

### Semantic Tokens

- Success: green family with AA contrast
- Warning: amber family with AA contrast
- Error: red family with AA contrast
- Info: blue/teal family with AA contrast

### Spacing Scale

- 4, 8, 12, 16, 24, 32, 48 px baseline scale.

### Typography

- Base body size: 16 px minimum.
- Defined heading scale (H1–H6) with clear hierarchy.

## Migration Plan from Bootstrap

1. Inventory current Bootstrap-dependent CSS classes and components in `HelpBoard.Web`.
2. Introduce tokenized foundation styles and framework primitives in parallel.
3. Replace layout/navigation Bootstrap usage first.
4. Replace tables, alerts, and button styling with tokenized components.
5. Remove Bootstrap asset references from `wwwroot/index.html` once migration is complete.
6. Validate accessibility and visual regressions before each migration phase is finalized.

## Dependencies

- Design token governance (who approves changes and naming).
- Accessibility review checklist to apply during implementation.

## Risks

- Incomplete migration can leave mixed visual patterns and inconsistent UX.
- Internal shared components can drift if token, icon wrapper, and accessibility rules are not enforced consistently.
- Color choices may fail contrast in edge combinations without strict token governance.

## Copilot Governance

### Candidate Instructions

- Require every new shared UI component to consume approved design tokens and semantic token aliases instead of hard-coded colors, spacing, elevation, or typography values.
- Require Lucide usage through the approved internal icon wrapper so sizing, stroke treatment, and accessible labeling remain consistent.
- Require accessibility notes for keyboard flow, focus treatment, semantic landmarks, heading structure, and non-color status communication whenever new foundational components are introduced.
- Require Bootstrap avoidance for all new shared UI work, and explicitly document any temporary migration-only Bootstrap dependency until it is removed.

### Candidate Skills

- UI foundation review skill to verify approved token usage, Lucide icon-wrapper alignment, accessibility notes, and Bootstrap avoidance before merge.
- Accessibility regression skill for shared components, layouts, and navigation patterns.

### Reusable Prompts

- "Review this UI change for approved token compliance, Lucide icon-wrapper consistency, accessibility notes, and Bootstrap avoidance."
- "List the accessibility checks that must pass for this new shared component before it can be accepted."
- "Identify any new shared UI surface that should become a repository instruction or component standard."

## Acceptance Criteria

- [x] Bootstrap is not used by newly implemented UI components.
- [x] Lucide icon usage is standardized through a documented approach.
- [x] Teal/purple token set is defined and adopted across foundational components.
- [x] Core components meet keyboard navigation and focus visibility requirements.
- [x] WCAG 2.2 AA checks are documented and pass for foundational UI surfaces.
- [x] A phased migration plan exists and is ready to execute.
- [x] Copilot governance artifacts are defined for UI foundation work, including token, icon, accessibility, and Bootstrap-migration guardrails.

Status: Complete (validated 2026-06-10).

## Resolved Decisions

- Dark mode and additional themes follow in Feature 008; Feature 001 covers only the default teal/purple foundation.
