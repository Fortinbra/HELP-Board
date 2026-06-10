---
applyTo: "**/*.cs"
---

# .NET Best Practices

Follow Microsoft's official recommendations for .NET and ASP.NET Core development.

---

## Dependency Injection

- Register all services with the built-in `IServiceCollection` / `IServiceProvider` container.
- Choose the correct **lifetime**:
  - `AddSingleton<T>` — one instance for the application lifetime (thread-safe types, configuration, caches).
  - `AddScoped<T>` — one instance per HTTP request / unit of work (repositories, DbContext).
  - `AddTransient<T>` — new instance every time (lightweight, stateless services).
- **Never resolve services from the constructor** using `IServiceProvider.GetService<T>()`; prefer constructor injection.
- Avoid captive dependencies: a singleton must not depend on a scoped or transient service.

```csharp
// Program.cs — correct lifetimes
builder.Services.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();
builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();
```

---

## Async / Await

- Use `async`/`await` throughout the entire call stack — never mix synchronous and asynchronous code.
- **Never block** on async code with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.
- Return `Task` or `Task<T>` (or `ValueTask`/`ValueTask<T>` for hot paths) from all async methods.
- Accept a `CancellationToken` parameter in every async public method and propagate it to all I/O calls.
- Use `ConfigureAwait(false)` in library / domain code (non-ASP.NET Core layers) to avoid deadlocks.

```csharp
public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    return await _db.Orders
        .AsNoTracking()
        .FirstOrDefaultAsync(o => o.Id == id, ct)
        .ConfigureAwait(false);
}
```

---

## Structured Logging

- Use `ILogger<T>` from `Microsoft.Extensions.Logging` exclusively — never `Console.WriteLine` in production code.
- Use **log message templates** (not string interpolation) so that structured logging sinks capture property values.
- Log at the appropriate level: `Trace`/`Debug` for diagnostics, `Information` for business events, `Warning` for recoverable issues, `Error`/`Critical` for faults.
- Include relevant correlation identifiers (e.g., order ID, user ID) as named properties.

```csharp
// Bad — string interpolation loses structured data
_logger.LogInformation($"Order {order.Id} placed by {userId}");

// Good — structured logging template
_logger.LogInformation("Order {OrderId} placed by {UserId}", order.Id, userId);
```

---

## Configuration

- Use `IOptions<T>`, `IOptionsSnapshot<T>`, or `IOptionsMonitor<T>` to bind strongly-typed settings.
- **Never inject `IConfiguration` directly** into domain services; use typed options instead.
- Validate options at startup using `ValidateDataAnnotations()` or `Validate()` to fail fast on misconfiguration.

```csharp
// appsettings.json section: "Email": { "SmtpHost": "...", "Port": 587 }

public class EmailOptions
{
    [Required] public string SmtpHost { get; init; } = string.Empty;
    [Range(1, 65535)] public int Port { get; init; }
}

// Registration
builder.Services
    .AddOptions<EmailOptions>()
    .BindConfiguration("Email")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Usage
public class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;
}
```

---

## Nullable Reference Types

- Enable nullable reference types in every project: `<Nullable>enable</Nullable>` in the `.csproj`.
- Annotate all APIs clearly: use `?` for nullable return types and parameters; use `!` (null-forgiving) only when the compiler cannot infer non-nullability but you are certain.
- Use `ArgumentNullException.ThrowIfNull(param)` at the top of public methods that must not receive null.

---

## Entity Framework Core

- Use `AsNoTracking()` on all read-only queries.
- Avoid lazy loading in web applications; use explicit `.Include()` to control query shape.
- Never expose `IQueryable` outside the repository layer — materialise the query inside the repository.
- Use **migrations** to manage schema changes; never modify a deployed migration.

---

## Exception Handling in ASP.NET Core

- Use global exception handling middleware (`UseExceptionHandler` / `IProblemDetailsService`) — avoid `try/catch` blocks in controllers.
- Return [Problem Details](https://www.rfc-editor.org/rfc/rfc9457) (RFC 9457) for all API errors.
- Create domain-specific exception types that carry enough context for logging and problem-detail generation.

---

## Performance Considerations

- Use `Span<T>` / `Memory<T>` for buffer manipulation to avoid heap allocations on hot paths.
- Pool objects with `ArrayPool<T>` or `ObjectPool<T>` where allocation cost matters.
- Avoid `async void`; use `async Task` instead (except for event handlers where `async void` is unavoidable).
- Use `IAsyncEnumerable<T>` for streaming large result sets rather than materialising them into a `List<T>`.
