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
| Feature 002: Dashboard Experience | 90 | 3.0 | 0.85 | 4.0 | 57.38 | P2 |
| Feature 003: Strategic Plan Tracker | 70 | 2.5 | 0.75 | 4.0 | 32.81 | P3 |
| Feature 004: Project Boards | 75 | 2.0 | 0.70 | 4.5 | 23.33 | P4 |

## Priority Order for Initial Execution
1. **P1** — Feature 001: UI Foundation & Theme System
2. **P2** — Feature 002: Dashboard Experience
3. **P3** — Feature 003: Strategic Plan Tracker
4. **P4** — Feature 004: Project Boards

## Update Guidance
- Re-score features whenever scope, dependencies, or team capacity changes.
- Keep the score inputs visible so prioritization rationale remains auditable.
- Use this document as the source of truth for sequencing future implementation work.
