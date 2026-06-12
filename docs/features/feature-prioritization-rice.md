# Feature Prioritization (RICE)

## Method
This document uses the RICE model to prioritize feature delivery:

- **Reach**: How many users or workflows are affected in a release period.
- **Impact**: Relative effect on user value and business outcomes.
- **Confidence**: Certainty in estimates and requirements quality.
- **Effort**: Relative implementation effort.

### Scoring Formula
**RICE Score = (Reach × Impact × Confidence) / Effort**

## Scoring Fields
Use these fields for updates as requirements mature:

- Reach (numeric)
- Impact (numeric; suggested scale: 0.25, 0.5, 1, 2, 3)
- Confidence (numeric percentage as decimal, e.g., 0.7)
- Effort (numeric person-month equivalent)
- Calculated RICE Score
- Priority Tier (P1, P2, P3, P4)

## Current Feature Priorities

| Feature | Reach | Impact | Confidence | Effort | RICE Score | Priority |
|---|---:|---:|---:|---:|---:|---|
| Feature 001: UI Foundation & Theme System | 95 | 3.0 | 0.90 | 2.5 | 102.60 | P1 |
| Feature 009: PostgreSQL & EF Core Integration | 95 | 3.0 | 0.90 | 3.0 | 85.50 | P1 |
| Feature 007: Feature Toggle System | 88 | 2.2 | 0.85 | 2.6 | 63.26 | P1 |
| Feature 002: Dashboard Experience | 90 | 2.6 | 0.82 | 3.8 | 50.49 | P2 |
| Feature 006: CI/CD and Release Deployment | 85 | 2.4 | 0.78 | 3.2 | 49.73 | P2 |
| Feature 003: Strategic Plan Tracker | 68 | 2.1 | 0.72 | 4.2 | 24.48 | P3 |
| Feature 004: Project Boards | 70 | 1.9 | 0.68 | 4.6 | 19.67 | P3 |
| Feature 008: Theme Expansion | 65 | 1.4 | 0.72 | 3.8 | 17.24 | P4 |
| Feature 005: Authentication and Authorization | 55 | 1.2 | 0.40 | 4.8 | 5.50 | P4 |

## Priority Order for Initial Execution
1. **P1** — Feature 001: UI Foundation & Theme System
2. **P1** — Feature 009: PostgreSQL & EF Core Integration
3. **P1** — Feature 007: Feature Toggle System
4. **P2** — Feature 002: Dashboard Experience
5. **P2** — Feature 006: CI/CD and Release Deployment
6. **P3** — Feature 003: Strategic Plan Tracker (hidden by default and iterated behind feature toggle until product-ready)
7. **P3** — Feature 004: Project Boards
8. **P4** — Feature 008: Theme Expansion
9. **P4** — Feature 005: Authentication and Authorization (deferred while Google integration direction remains uncertain)

## Update Guidance
- Re-score features whenever scope, dependencies, or team capacity changes.
- Keep the score inputs visible so prioritization rationale remains auditable.
- Use this document as the source of truth for sequencing future implementation work.
