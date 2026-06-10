---
applyTo: "**/*.cs"
---

# Clean Code Guidelines for .NET / C#

---

## Meaningful Names

- Use **intention-revealing names**: a reader should understand what a variable, method, or class does without reading its implementation.
- Avoid abbreviations unless they are universally understood in the domain (e.g., `Id`, `Url`, `Html`).
- Use **nouns** for classes and variables; use **verbs** for methods.
- Avoid generic names (`Manager`, `Helper`, `Util`, `Data`); prefer names that describe the specific responsibility.
- Boolean variables and properties should read as a predicate: `isActive`, `hasPermission`, `canRetry`.

```csharp
// Bad
var d = DateTime.UtcNow.AddDays(7);
bool flag = user.r > 0;

// Good
var passwordExpiryDate = DateTime.UtcNow.AddDays(7);
bool hasRemainingAttempts = user.RemainingLoginAttempts > 0;
```

---

## Small, Focused Methods

- A method should **do one thing** and do it well.
- Aim for methods that fit on a single screen (~20 lines is a useful upper bound, not a hard rule).
- Avoid deep nesting — extract nested `if`/`foreach` blocks into well-named helper methods.
- Use **early returns** (guard clauses) to reduce nesting and remove `else` branches.

```csharp
// Bad — deep nesting, multiple concerns
public async Task ProcessOrderAsync(Order? order)
{
    if (order != null)
    {
        if (order.Items.Count > 0)
        {
            foreach (var item in order.Items)
            {
                if (item.Stock > 0)
                {
                    // ... 20 more lines
                }
            }
        }
    }
}

// Good — guard clauses + extracted methods
public async Task ProcessOrderAsync(Order? order)
{
    if (order is null) return;
    if (order.Items.Count == 0) return;

    foreach (var item in order.Items)
        await ProcessItemAsync(item);
}

private async Task ProcessItemAsync(OrderItem item)
{
    if (item.Stock <= 0) return;
    // focused logic here
}
```

---

## Don't Repeat Yourself (DRY)

- Every piece of knowledge should have a **single, unambiguous, authoritative representation** in the system.
- Before copying code, extract it into a shared method, extension method, or service.
- Be careful not to over-apply DRY at the cost of forced abstractions — duplicate code that happens to look the same is not always the same _concept_.

---

## No Magic Numbers or Strings

- Replace literal constants with **named constants** or configuration values.

```csharp
// Bad
if (user.FailedAttempts >= 5)
    LockAccount(user);

// Good
private const int MaxFailedLoginAttempts = 5;

if (user.FailedAttempts >= MaxFailedLoginAttempts)
    LockAccount(user);
```

---

## Comments

- Code should be **self-documenting**; comments should explain **why**, not what.
- If you feel the need to write a "what" comment, rename or refactor the code instead.
- Use XML doc-comments (`///`) on all `public` and `internal` APIs.
- Never commit commented-out code — use version control to preserve history.

```csharp
// Bad — explains what, not why
// Add 1 to the count
count++;

// Good — explains a non-obvious business rule
// Accounts for the off-by-one error in the legacy billing system
count++;
```

---

## Error Handling

- Use **exceptions for exceptional conditions**, not for control flow.
- Throw specific exception types; catch the most specific type possible.
- Always include a meaningful message in exceptions that will be logged or surfaced.
- Use `Result<T>` / discriminated unions for expected failure paths in domain logic where throwing is inappropriate.

```csharp
// Bad — swallows all exceptions
try { /* ... */ } catch (Exception) { }

// Good — catch only what you can handle, re-throw with context when needed
try
{
    await _repo.SaveAsync(order, ct);
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict saving order {OrderId}", order.Id);
    throw new OrderConflictException(order.Id, ex);
}
```

---

## Code Organisation

- Keep related code together; group fields, constructors, properties, public methods, then private methods — in that order.
- Limit file length; a file with more than ~300 lines is a signal that the type is doing too much.
- One type per file; name the file to match the primary type it contains.
- Remove unused `using` directives, variables, and parameters.
