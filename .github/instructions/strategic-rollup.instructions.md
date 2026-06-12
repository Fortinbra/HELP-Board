---
applyTo: "src/**/*.{cs,razor,razor.cs}"
description: "Use when implementing or modifying strategic progress/health roll-up logic to require explicit calculation documentation and tests."
---

# Strategic Roll-Up Governance

## Use When

Use when strategic hierarchy, progress aggregation, or health/status roll-up behavior changes.

## Rules

- Document the exact roll-up path and formula assumptions (milestone -> initiative -> objective) in the related change notes or feature documentation.
- Describe tie-breakers, null/missing-data handling, and rounding rules when roll-up behavior is altered.
- Every roll-up logic change must include tests that prove expected objective-level outcomes from child updates.

## Enforcement Expectations

- Pull requests that change roll-up behavior must include both: updated roll-up description and updated/added tests.
- Treat undocumented roll-up changes as incomplete implementation work.
