---
applyTo: "src/HelpBoard.Web/**/*.razor"
---

# Blazor UI Code-Behind Pattern

Use the code-behind pattern for all Blazor component C# behavior in the UI project.

## Rule

- Do not place component logic inside `@code` blocks in `.razor` files.
- Keep `.razor` files focused on markup and Razor directives only.
- Put component behavior in a paired partial class file with the same name and `.razor.cs` extension.

## What Belongs in Code-Behind

- Event handlers and command methods
- Lifecycle methods (`OnInitialized`, `OnInitializedAsync`, `OnParametersSet`, etc.)
- State fields and private backing data
- Computed properties and helper methods used by markup

## Pairing Requirement

When a component contains behavior, maintain a one-to-one pairing:

- `Component.razor`
- `Component.razor.cs`

## Allowed Exception

Truly static presentational components with no C# behavior may remain as `.razor`-only files.

## Rationale

This rule exists because xUnit and bUnit instrumentation currently has limitations with inline component logic. Keeping behavior in code-behind improves testability and instrumentation reliability.
