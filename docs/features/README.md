# Feature Documents

This directory contains implementation-ready feature documents for HELP-Board.

The [global application requirements](../global-requirements.md) define the application-wide reporting purpose, architectural constraints, and confirmed scope. These feature documents describe individual delivery slices, not completion of the global requirements.

## Documents

- `feature-template.md` — Shared template for all feature documents.
- [Database guidance](../database.md) — PostgreSQL/EF Core persistence foundation and migration workflow.
- [Feature 011: Shared Dashboard Builder and Data Sources](feature-011-dashboard-data-sources.md) - Shared grid-based dashboards first; widget data-source integrations follow.
- [Feature backlog](backlog.md) — Deferred portions of features to revisit later.
- `feature-prioritization-rice.md` — RICE-based feature prioritization and scoring.

## On Hold

- [Feature 003: Strategic Plan Tracker](on-hold/feature-003-strategic-plan-tracker.md) — Strategic objective and progress tracking capability, deferred for future work.
- [Feature 004: Project Boards](on-hold/feature-004-project-boards.md) — Project board creation and execution tracking capability, deferred for future work.
- [Feature 006: CI/CD and Release Deployment](on-hold/feature-006-ci-cd-and-release-deployment.md) — Delivery pipeline planning and automation, deferred for future work.
- [Feature 010: Director's Report](on-hold/feature-010-directors-report.md) — Dedicated report and publication workflows, deferred while dashboard building is prioritized.
