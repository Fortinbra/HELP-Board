# Feature 001: UI Foundation & Theme System

## Goal
Establish a reusable, accessible UI foundation for HELP-Board with a teal/purple design language, a non-Bootstrap component strategy, and consistent iconography using Lucide.

## User Value
Users get a clear, modern, and accessible interface that makes work status easy to understand at a glance. Teams gain a stable visual system that reduces inconsistency as new features are added.

## Scope
### In Scope
- Select and adopt a Blazor-compatible UI framework that is **not Bootstrap**.
- Define a design token system for color, spacing, typography, elevation, and interactive states.
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
- Static at runtime for initial release, with room for future user-selectable themes.

## Accessibility (WCAG)
- Meet WCAG 2.2 AA color contrast targets for text, icons, controls, and chart-adjacent UI.
- Ensure visible and consistent focus indicators for all interactive elements.
- Provide full keyboard navigation for global navigation and all core components.
- Use semantic HTML landmarks and heading hierarchy across layouts.
- Ensure icon-only actions include accessible names.
- Do not rely on color alone to communicate status.

## Framework and Library Decisions
- UI framework must be Blazor-native or Blazor-friendly and explicitly exclude Bootstrap.
- Lucide is the standard icon set for navigation, actions, and status-adjacent iconography.
- Any selected component library must support accessibility and theming extensibility.

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
- Final UI framework decision and package integration.
- Design token governance (who approves changes and naming).
- Accessibility review checklist to apply during implementation.

## Risks
- Incomplete migration can leave mixed visual patterns and inconsistent UX.
- Some third-party components may not meet accessibility requirements by default.
- Color choices may fail contrast in edge combinations without strict token governance.

## Copilot Governance
### Candidate Instructions
- Require every new shared UI component to consume approved design tokens instead of hard-coded colors, spacing, or typography values.
- Require Bootstrap usage checks in any new UI work until the migration is complete.
- Require accessibility notes for keyboard flow, focus treatment, and semantic structure whenever new foundational components are introduced.

### Candidate Skills
- UI foundation review skill to verify token usage, Lucide alignment, and Bootstrap avoidance before merge.
- Accessibility regression skill for shared components, layouts, and navigation patterns.

### Reusable Prompts
- "Review this UI change for design-token compliance, Lucide consistency, and Bootstrap avoidance."
- "List the accessibility checks that must pass for this new shared component before it can be accepted."
- "Identify any new shared UI surface that should become a repository instruction or component standard."

## Acceptance Criteria
- [ ] Bootstrap is not used by newly implemented UI components.
- [ ] Lucide icon usage is standardized through a documented approach.
- [ ] Teal/purple token set is defined and adopted across foundational components.
- [ ] Core components meet keyboard navigation and focus visibility requirements.
- [ ] WCAG 2.2 AA checks are documented and pass for foundational UI surfaces.
- [ ] A phased migration plan exists and is ready to execute.
- [ ] Copilot governance artifacts are defined for UI foundation work, including token, icon, accessibility, and Bootstrap-migration guardrails.

## Open Questions
- Which specific non-Bootstrap UI framework best matches maintainability and accessibility priorities?
- Should dark mode be included in initial token design or follow as a separate feature?
