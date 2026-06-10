# GitHub Copilot Instructions — .NET Projects

This repository contains shared Copilot instructions that apply to any .NET project. The instructions promote **SOLID principles**, **Clean Code**, **Test-Driven Development (TDD)**, and **Microsoft's official .NET recommendations**. They are intentionally project-agnostic and are meant to be combined with a repository's own local instructions.

---

## Core Philosophy

- Write code that is **readable first, clever second**. Future maintainers (including yourself) should be able to understand any piece of code without needing to trace through multiple levels of indirection.
- Apply SOLID principles consistently to keep code easy to change and extend.
- Treat tests as **first-class citizens**: write them before or alongside production code, and keep them as clean as the code they cover.
- Follow Microsoft's official .NET and C# guidelines unless a project's local instructions explicitly override them.
- Prefer the **latest stable .NET LTS version** and idiomatic modern C# features (nullable reference types, records, pattern matching, file-scoped namespaces, primary constructors, etc.).
- If asked to implement something that **contradicts these guidelines**, ask clarifying questions before proceeding and suggest alternative approaches that remain compliant. Do not silently implement a guideline violation.

---

## SOLID Principles (Summary)

| Principle | One-line rule |
|-----------|--------------|
| **S**ingle Responsibility | A class or method has exactly one reason to change. |
| **O**pen/Closed | Open for extension, closed for modification — use abstraction and polymorphism. |
| **L**iskov Substitution | Subtypes must be fully substitutable for their base types without altering program correctness. |
| **I**nterface Segregation | Prefer many small, focused interfaces over one large general-purpose interface. |
| **D**ependency Inversion | Depend on abstractions (interfaces), not on concrete implementations; inject dependencies. |

> Detailed guidance is in `.github/instructions/solid-principles.instructions.md`.

---

## Clean Code (Summary)

- Use **intention-revealing names** for all identifiers; never use abbreviations unless they are universally understood domain terms.
- Methods should do **one thing** and fit on a single screen (~20 lines maximum as a guide).
- Eliminate duplication (DRY). Extract shared logic into well-named helper methods or services.
- Avoid magic numbers and strings — declare them as named constants or configuration values.
- Comments should explain **why**, never **what** — if the code needs a "what" comment, it needs to be refactored.

> Detailed guidance is in `.github/instructions/clean-code.instructions.md`.

---

## Test-Driven Development (Summary)

- Follow the **Red → Green → Refactor** cycle: write a failing test, make it pass with the simplest code possible, then clean up.
- Name tests using the **`MethodName_Scenario_ExpectedBehavior`** convention.
- Structure every test with **Arrange / Act / Assert** sections, using blank lines to separate them.
- Unit tests must be **fast, isolated, and deterministic** — mock all external dependencies.
- Aim for high coverage of business logic; do not chase 100% line coverage at the expense of test quality.

> Detailed guidance is in `.github/instructions/tdd.instructions.md`.

---

## .NET Best Practices (Summary)

- Use the built-in **Dependency Injection** container (`IServiceCollection`); register services with the appropriate lifetime.
- Use `async`/`await` throughout the call stack; never block with `.Result` or `.Wait()`.
- Use `ILogger<T>` for structured logging; never use `Console.WriteLine` in production code.
- Leverage `CancellationToken` propagation for all I/O-bound operations.
- Use `IOptions<T>` for configuration rather than reading `IConfiguration` directly in services.

> Detailed guidance is in `.github/instructions/dotnet-best-practices.instructions.md`.

---

## C# Coding Style (Summary)

- Use **file-scoped namespaces** (`namespace MyApp;`).
- Use **`var`** when the type is obvious from the right-hand side; use explicit types otherwise.
- Use **expression-bodied members** for simple one-liners.
- Use **record types** for immutable data transfer objects.
- Enable **nullable reference types** (`<Nullable>enable</Nullable>`) in every project.

> Detailed guidance is in `.github/instructions/csharp-style.instructions.md`.

---

## How These Instructions Combine with Local Instructions

When a project includes its own `.github/copilot-instructions.md` or `.github/instructions/*.instructions.md` files, those **extend and override** the instructions in this template repository. Local instructions take precedence for project-specific concerns (e.g., framework choices, database access patterns, naming conventions specific to the domain).
