# Accessibility Decisions (Feature 001)

This document records accessibility decisions for the UI foundation and where they are implemented.

## Decision 1: Icon Semantics (Decorative vs Informative)

### Decision

- Decorative icons are hidden from assistive technology.
- Informative icons are exposed with a meaningful label.

### Rationale

- Prevent noisy or duplicate announcements for purely visual decoration.
- Preserve meaning for icon-only or status-relevant symbols.

### Implementation References

- Icon role and ARIA behavior in [src/HelpBoard.Web/Components/AppIcon.razor](src/HelpBoard.Web/Components/AppIcon.razor#L1).
- Decorative icon usage examples in [src/HelpBoard.Web/Pages/Home.razor](src/HelpBoard.Web/Pages/Home.razor#L48).
- Labeled link with icon context in [src/HelpBoard.Web/Layout/MainLayout.razor](src/HelpBoard.Web/Layout/MainLayout.razor#L15).

## Decision 2: Focus Strategy for Navigation and Route Changes

### Decision

- Provide a skip link to bypass navigation.
- Move focus on route changes with FocusOnNavigate.
- Keep main content programmatically focusable via tabindex="-1".

### Rationale

- Keyboard and assistive tech users can reach primary content quickly.
- Route transitions are announced in context without requiring manual refocus.

### Implementation References

- Route focus transfer in [src/HelpBoard.Web/App.razor](src/HelpBoard.Web/App.razor#L4).
- Skip link and main target in [src/HelpBoard.Web/Layout/MainLayout.razor](src/HelpBoard.Web/Layout/MainLayout.razor#L4).
- Skip-link styling/behavior in [src/HelpBoard.Web/wwwroot/css/app.css](src/HelpBoard.Web/wwwroot/css/app.css#L146).

## Decision 3: Focus Indicator Consistency with :focus-visible

### Decision

- Use :focus-visible for keyboard focus indication.
- Use one shared focus ring token for consistency.

### Rationale

- Avoid visual noise for pointer interactions while preserving strong keyboard affordance.
- Keep focus treatment consistent across controls and components.

### Implementation References

- Focus token and global selector in [src/HelpBoard.Web/wwwroot/css/app.css](src/HelpBoard.Web/wwwroot/css/app.css#L49).
- Button/input focus application in [src/HelpBoard.Web/wwwroot/css/app.css](src/HelpBoard.Web/wwwroot/css/app.css#L248).

## Decision 4: Status Communication Uses Color Plus Text

### Decision

- Status and priority are communicated through color and explicit text labels (and icons where helpful).

### Rationale

- Meets non-color dependency expectations and improves clarity for all users.

### Implementation References

- Dashboard note and badge composition in [src/HelpBoard.Web/Pages/Home.razor](src/HelpBoard.Web/Pages/Home.razor#L113).
- Progress bar includes textual percentage and ARIA values in [src/HelpBoard.Web/Pages/Home.razor](src/HelpBoard.Web/Pages/Home.razor#L103).
- Alert/status live regions in [src/HelpBoard.Web/Pages/Home.razor](src/HelpBoard.Web/Pages/Home.razor#L28).

## Review Trigger

Revisit these decisions when changing shell navigation, icon component behavior, or shared focus/status tokens.
