# Feature Documents

This directory contains implementation-ready feature documents for HELP-Board.

The [global application requirements](../global-requirements.md) define the application-wide reporting purpose, architectural constraints, and confirmed scope. These feature documents describe individual delivery slices, not completion of the global requirements.

## Documents

- `feature-template.md` — Shared template for all feature documents.
- `feature-001-ui-foundation-and-theme-system.md` — UI framework, theming, icon strategy, and WCAG-first UX foundation.
- `feature-002-dashboard-experience.md` — Main post-login dashboard experience for making work visible.
- `feature-003-strategic-plan-tracker.md` — Strategic objective and progress tracking capability.
- `feature-004-project-boards.md` — Project board creation and execution tracking capability.
- `feature-005-authentication-and-authorization.md` — Google Workspace-first authentication with Authentik fallback and RBAC/security requirements.
- `feature-006-ci-cd-and-release-deployment.md` — PR-required validation checks and release-triggered deployment pipeline requirements.
- `feature-007-feature-toggle-system.md` — Local-first, OSS-friendly feature flag strategy for features and themes.
- `feature-008-theme-expansion.md` — VS Code-inspired Light/Dark and HELP standard themes, system preference/persistence, and optional Geocities/Minimalist/Western themes.
- `feature-009-postgres-efcore-integration.md` — PostgreSQL and EF Core persistence foundation and migration workflow.
- [Feature 010: Director's Report](feature-010-directors-report.md) - First form-driven report, drafts/publication, regular/ad hoc meeting records, fixed snapshots, corrections, and historic trends.
- `feature-prioritization-rice.md` — RICE-based feature prioritization and scoring.
