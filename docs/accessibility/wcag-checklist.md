# WCAG 2.2 AA Checklist for Foundational UI Surfaces

Use this checklist for app shell, navigation, cards, tables, forms, badges, alerts, and icon-bearing actions.

## Baseline Criteria

| Area | Requirement (WCAG 2.2 AA) | Threshold / Rule | Check |
|---|---|---|---|
| Text contrast | Normal and large text meet contrast | Normal text: >= 4.5:1. Large text (>= 24px regular or >= 18.66px bold): >= 3:1 | [ ] |
| Non-text contrast | UI boundaries, focus indicators, and meaningful icons are visible | >= 3:1 against adjacent colors | [ ] |
| Focus visible | Keyboard focus is always visible | Every interactive control shows a visible focus state | [ ] |
| Focus appearance | Focus indicator has enough visibility | Indicator area/offset and contrast are clearly visible on all themes | [ ] |
| Keyboard access | All interaction is keyboard operable | No keyboard trap, logical tab order, Enter/Space behavior where applicable | [ ] |
| Semantic structure | Landmarks and headings are meaningful | Use header/nav/main/footer and ordered heading hierarchy | [ ] |
| Icon labeling | Icon-only actions are named | Icon-only controls have accessible names (aria-label or visible text) | [ ] |
| Decorative icons | Decorative graphics are hidden from AT | Use aria-hidden="true" and no redundant spoken label | [ ] |
| Status communication | State is not color-only | Pair color with text/icon/shape (for success, warning, error, info) | [ ] |
| Dynamic updates | Important updates are announced appropriately | Use role="status"/aria-live for passive updates and role="alert" for urgent messages | [ ] |

## Validation Methods

## 1) Manual Keyboard Pass

1. Start at the browser address bar and tab into the app.
2. Confirm skip link appears and jumps to main content.
3. Tab through nav, header actions, and page controls.
4. Verify each focused element has a visible focus ring.
5. Confirm no trap; Shift+Tab returns correctly.
6. Trigger primary actions with Enter/Space where expected.

Pass when full user flow is keyboard-completable without ambiguity.

## 2) Screen Reader Smoke Checks

Run at least one desktop reader (NVDA or VoiceOver).

1. Read landmarks list; confirm navigation and main region are present.
2. Navigate by headings; confirm structure matches visual layout.
3. Verify icon-only controls announce meaningful names.
4. Trigger loading/error/status states and confirm announcements are sensible.

Pass when core flow announcements are accurate and concise.

## 3) Automated Checks

- Run axe DevTools on changed pages/components.
- Run WAVE on the same surfaces.
- Treat critical/serious issues as merge blockers.

Pass when blockers are resolved or documented with approved exception.

## PR Checklist Template (Component Changes)

Copy into PR description for any foundational component/layout change.

- [ ] Keyboard-only pass completed on changed surfaces.
- [ ] Focus-visible ring is present and consistent.
- [ ] Landmark/heading semantics remain correct.
- [ ] Icon-only actions have accessible names.
- [ ] Decorative icons are hidden from assistive tech.
- [ ] Status indicators use color plus text/icon cues.
- [ ] Screen reader smoke check performed.
- [ ] axe and WAVE checks completed; blockers resolved.
- [ ] Any exception/risk is documented with follow-up owner/date.
