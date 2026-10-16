# Feature 008: Theme Expansion

## Goal

Deliver standard VS Code-inspired Light and Dark themes plus a distinct HELP purple/teal theme, with optional Geocities, Minimalist, and Western themes retained for later delivery. Follow the current [global theme requirements](../global-requirements.md#101-themes-branding-and-shared-ui); the earlier teal/purple foundation is not the default theme policy or authoritative HELP brand palette.

## User Value

Users can choose presentation styles that match brand, comfort, and context while retaining clear information hierarchy and operability.

## Scope

### In Scope

- Define required theme set for initial expansion.
- Define theme-specific visual constraints and interaction behavior.
- Define accessibility and reduced-motion expectations across themes.
- Define integration with feature toggle controls for new themes.

### Out of Scope

- User-generated custom theme builder.
- Full branding overhaul beyond the identified theme set.

## UX

### User Flows

- With no explicit selection, the app follows the system light/dark preference, including preference changes.
- User chooses Light, Dark, HELP, or returns to system selection; explicit choices override system preference and persist.
- Admin enables optional novelty themes (Geocities, Western) through feature toggles.
- User selects the optional Minimalist theme when enabled for reduced visual density.

### Key Screens / Components

- Theme selector in user preferences.
- Theme preview samples (colors, typography, controls, cards).
- Theme-aware component tokens for navigation, cards, forms, and status indicators.

### States

- Theme loading/applied.
- Theme unavailable due to feature flag disabled.
- Reduced-motion mode overriding high-motion behavior.

## Data Needs

### Data Sources

- Central design token definitions.
- User preference setting.
- Feature toggle state for theme availability.

### Data Model Considerations

- Theme registry should include key, display name, token set, and availability flag.
- User profile should persist selected theme preference.
- Reduced-motion preference should be respected globally, including high-motion themes.
- A disabled or unavailable saved theme must fall back to an available standard theme; disabled themes must not be selectable.

### Data Freshness

- Theme changes apply immediately in-session.
- Persisted preferences load on sign-in/session start.

## Standard Theme Set

- **Light:** VS Code-inspired default light presentation.
- **Dark:** VS Code-inspired default dark presentation.
- **HELP:** separate organizational theme using the official HELP purple/teal brand references and approved logo assets.

Initial selection follows the system preference unless a user has saved an explicit choice. Use the [HELP website](https://www.helpcenterofaustin.org/) as the brand source. Observed website colors are purple `#7400B3` and teal `#00B4D0`; final semantic tokens must be contrast-tested and approved. The [global requirements](../global-requirements.md#101-themes-branding-and-shared-ui) record the published logo and palette provenance. Package approved assets locally; do not depend on runtime hotlinks.

The initial hello-world shell implements browser-local theme persistence under `helpboard.theme`, system-change tracking, and separate approved `LightTheme`/`DarkTheme`/`HelpTheme` flags. It uses locally hosted HELP branding and VS Code-inspired neutral light/dark surfaces. Authentication-linked preferences, full legacy-screen theme migration, exact-reference approval, and chart theme parity remain future acceptance work; shell implementation alone does not complete this feature.

## Optional Planned Themes

- **Geocities-Inspired:** intentionally bright and nostalgic presentation with animated accents, decorative typography options, and saturated colors.
- **Minimalist:** low-noise presentation with restrained typography, spacing, and visual chrome.
- **Western:** horse/cowboy-inspired palette and motifs aligned with therapeutic riding center identity.

## Accessibility and Motion Requirements

- All themes must meet WCAG 2.2 AA contrast and keyboard interaction requirements.
- All themes must preserve semantic structure and readable content hierarchy.
- Geocities-inspired theme must support strict reduced-motion behavior and an option to disable non-essential animations.
- Decorative fonts and effects must never reduce legibility below baseline standards.

## Dependencies

- Feature 001 UI Foundation & Theme System token architecture.
- Feature 007 Feature Toggle System for theme availability controls.
- Accessibility review checklist and visual QA coverage.

## Positioning Relative to Feature 001

- Feature 001 delivered the original teal/purple token foundation, shell, controls, icons, accessibility baseline, and Bootstrap migration plan.
- Feature 008 builds on that foundation and owns the current Light/Dark/HELP standard set, system-selection behavior, preference persistence, and optional theme variants.
- The standard theme set is a production requirement; historical low priority for novelty theme expansion does not defer it.

## Risks

- High-style themes can create readability or motion-sensitivity issues.
- Theme drift can occur if token governance is weak.
- Excessive theme branching can increase maintenance cost.

## Copilot Governance

### Candidate Instructions

- Require each new theme to map back to approved token slots, accessibility checks, and feature-toggle controls before implementation begins.
- Require theme changes to document reduced-motion behavior, readability protections, and any novelty-theme constraints.
- Require theme QA to cover parity of core components, status states, and preference persistence across enabled themes.

### Candidate Skills

- Theme governance skill to review token alignment, toggle integration, and consistency with the shared UI foundation.
- Theme accessibility and motion skill to review contrast, animation controls, and readability across theme variants.

### Reusable Prompts

- "Review this theme change for token drift, feature-toggle integration, and accessibility or reduced-motion regressions."
- "List the QA checks required before enabling this theme in a shared environment."
- "Identify which theme-specific rules should become permanent Copilot instructions or reusable prompts."

## Acceptance Criteria

- [ ] Light, Dark, and HELP standard themes and three optional planned themes are documented and implemented within the approved delivery phase.
- [ ] Light and Dark follow the selected VS Code references; HELP uses approved official brand assets and accessible tokens.
- [ ] Initial system preference, system changes, explicit override, persistence, return-to-system selection, and disabled-theme fallback are verified.
- [ ] Geocities-inspired theme constraints include animation and legibility controls.
- [ ] Minimalist and Western theme design expectations are documented.
- [ ] Accessibility and reduced-motion requirements are documented for all themes.
- [ ] Theme availability is documented as feature-toggle controlled.
- [ ] Copilot governance artifacts are defined for theme work, covering token alignment, toggle controls, and accessibility or motion guardrails.

## Open Questions

- Which exact VS Code default light/dark references and final HELP logo/token variants are approved?
- Should novelty themes be restricted by role or environment?
- How should theme QA regression coverage be prioritized over time?
