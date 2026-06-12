---
applyTo: "src/**/*.{cs,razor,razor.cs}"
description: "Use when implementing or modifying strategic ownership and update flows to enforce owner attribution and update metadata requirements."
---

# Strategic Accountability Governance

## Use When

Use when strategic entities, ownership assignment, or status/progress update workflows are created or changed.

## Rules

- Strategic objectives, initiatives, and milestones must keep explicit owner attribution (person, role, or team identity as defined by the feature).
- Status/progress updates must capture update metadata at minimum: who updated and when updated.
- Changes that affect ownership or update flows must document how attribution and update metadata are preserved.

## Enforcement Expectations

- Do not merge strategic update-flow changes that can remove or bypass owner/update metadata.
- Test or verify that update operations persist owner attribution and update metadata fields.
