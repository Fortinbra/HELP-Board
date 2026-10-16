# HELP-Board

HELP-Board is a reporting and data visualization dashboard for the HELP board of directors, built with an ASP.NET Core API hosting a Blazor WebAssembly frontend and PostgreSQL persistence through Entity Framework Core.

Planned production URL: <https://help.becauseimclever.com>. Board data requires authorized access.

This purpose-built application will run as a Docker container on PiTony (Raspberry Pi 5, 16 GB RAM). Standard themes are VS Code-inspired Light/Dark and HELP purple/teal, with system light/dark preference used until the user chooses a theme.

- [Global application requirements](docs/global-requirements.md) - Product scope, architecture, identity, reporting, visualization catalog, quality requirements, and open decisions.
- [Feature documents](docs/features/README.md) - Feature-level scope and implementation guidance.

## Initial UI baseline

Run `dotnet run --project src\HelpBoard.Web\HelpBoard.Web.csproj` to preview the standalone WASM shell locally. The default configuration enables the hello-world layout and System/Light/Dark/HELP themes. Sample report navigation is intentionally unavailable; sign-in is not implemented in this milestone. This preview does not start the API or connect to PostgreSQL.

The HELP logo is packaged from the [official website's published logo](https://static1.squarespace.com/static/5c32c0bd5ffd200348050279/t/679920a66510b00378ee444d/1738088614083/H.E.L.P.+Logo+Correct+Colors+-+transp+bkgrd_2022.png?format=500w), retrieved 2026-10-16. Browser-local theme persistence is not an account preference store.

Validate the shell with `dotnet test tests\HelpBoard.Web.Tests\HelpBoard.Web.Tests.csproj` and `node --test tests\HelpBoard.Web.Tests\theme.test.cjs`.

The repository also includes the shared Copilot templates described below.

## CoPilotTemplates

A collection of shared GitHub Copilot instructions, path-specific guidelines, and agent definitions for .NET development. These templates are **project-agnostic** — they apply to any .NET project and are designed to be combined with a repository's own local Copilot customisations.

---

## What's Included

| File | Purpose |
|------|---------|
| `.github/copilot-instructions.md` | Repository-wide Copilot instructions covering all core principles |
| `.github/instructions/solid-principles.instructions.md` | SOLID principles with C# examples — applied to all `*.cs` files |
| `.github/instructions/clean-code.instructions.md` | Clean Code guidelines — applied to all `*.cs` files |
| `.github/instructions/tdd.instructions.md` | TDD practices (Red-Green-Refactor, AAA, naming) — applied to test files |
| `.github/instructions/dotnet-best-practices.instructions.md` | Microsoft-recommended .NET patterns — applied to all `*.cs` files |
| `.github/instructions/csharp-style.instructions.md` | C# language-specific style guidelines — applied to all `*.cs` files |
| `AGENTS.md` | High-level agent instructions for AI agents working on .NET projects |

---

## Core Topics Covered

- **SOLID Principles** — Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Clean Code** — Meaningful names, small focused methods, DRY, no magic numbers, purposeful comments
- **Test-Driven Development** — Red-Green-Refactor, AAA structure, test naming, isolation, test doubles
- **.NET Best Practices** — Dependency injection lifetimes, async/await, structured logging, typed configuration, nullable reference types, EF Core patterns
- **C# Style** — File-scoped namespaces, naming conventions, `var` usage, modern features (records, pattern matching, primary constructors, collection expressions)

---

## How to Use These Templates

### Option 1 — Copy into Your Repository

Copy the `.github/` directory (and optionally `AGENTS.md`) into the root of your .NET project. Then add your own project-specific instructions alongside them. Local instruction files **extend and override** the shared templates.

### Option 2 — Reference as a Template Repository

1. On GitHub, click **Use this template** to create a new repository pre-populated with these files.
2. Add project-specific `.github/copilot-instructions.md` content and additional `*.instructions.md` files as needed.

---

## Extending the Templates

To add project-specific guidance without modifying the shared files:

1. Create (or edit) `.github/copilot-instructions.md` in your project with project-specific notes — they will be combined with the repository-wide instructions.
2. Add new `*.instructions.md` files under `.github/instructions/` for additional path-specific rules (e.g., rules that apply only to `*Controller.cs` files).
3. Edit `AGENTS.md` at the repository root to add project-specific agent behaviour rules.

---

## Principles

These templates follow:
- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Application Architecture Guidance](https://dotnet.microsoft.com/en-us/learn/dotnet/architecture-guides)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID) as applied to C# and .NET
- [Clean Code](https://www.goodreads.com/book/show/3735293-clean-code) principles by Robert C. Martin
- [Test-Driven Development](https://en.wikipedia.org/wiki/Test-driven_development) practices
