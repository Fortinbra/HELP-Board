# Agent Instructions — .NET / C# Projects

This file provides guidance for AI agents (GitHub Copilot, and compatible tools) working on any .NET repository that references these shared templates.

---

## What This Repository Is

`CoPilotTemplates` is a **shared template repository** containing GitHub Copilot instructions that apply to any .NET project. There is no production application code here — the deliverable is the set of `.github/copilot-instructions.md`, `.github/instructions/*.instructions.md`, and `AGENTS.md` files themselves.

---

## Guiding Principles

When generating or modifying code in any .NET project that uses these templates, always apply:

1. **SOLID principles** — see `.github/instructions/solid-principles.instructions.md`
2. **Clean Code** — see `.github/instructions/clean-code.instructions.md`
3. **TDD** — see `.github/instructions/tdd.instructions.md`
4. **.NET best practices** — see `.github/instructions/dotnet-best-practices.instructions.md`
5. **C# style** — see `.github/instructions/csharp-style.instructions.md`

---

## How to Apply These Instructions in a Target Repository

To use these templates in a .NET project, copy or reference the relevant instruction files into the target repository's `.github/` directory. Local instructions that live alongside these files **extend and override** the shared templates for project-specific concerns.

---

## Behaviour Rules for Agents

- **Prefer interfaces over concrete types** at API boundaries.
- **Always propagate `CancellationToken`** through every async call chain.
- **Write a test before writing production code** when adding new behaviour (TDD Red-Green-Refactor).
- **Never commit secrets, connection strings, or personal data** to the repository.
- **Keep changes focused and minimal** — address only the described issue; avoid unrelated refactoring in the same change set.
- When uncertain about the intended design, **ask a clarifying question** rather than making an assumption that could introduce architectural inconsistency.
- If asked to implement anything that **contradicts these guidelines**, ask clarifying questions before proceeding and suggest compliant alternative approaches. Do not silently implement a guideline violation.
- **Validate changes** by running the project's existing test suite; do not skip tests.

---

## Preferred Libraries and Frameworks

| Concern | Preferred choice |
|---------|-----------------|
| Web API | ASP.NET Core (Minimal API or controllers) |
| ORM | Entity Framework Core |
| Dependency Injection | `Microsoft.Extensions.DependencyInjection` (built-in) |
| Logging | `Microsoft.Extensions.Logging` + Serilog (structured sink) |
| Testing framework | xUnit |
| Assertion library | xUnit built-in assertions or FluentAssertions |
| Mocking | Moq or NSubstitute |
| Validation | `System.ComponentModel.DataAnnotations` + FluentValidation for complex rules |

Only introduce libraries not listed here if the project's local instructions or an explicit requirement calls for them.
