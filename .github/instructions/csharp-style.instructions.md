---
applyTo: "**/*.cs"
---

# C# Coding Style

Follow the [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) and the [.NET Runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md). The rules below summarise the most important points.

---

## File and Namespace Layout

- Use **file-scoped namespaces** (C# 10+):
  ```csharp
  namespace MyApp.Orders;  // file-scoped — no extra indentation level
  ```
- One **primary type per file**; name the file to match the type.
- Order the contents of a file as follows:
  1. `using` directives (system first, then third-party, then local — sorted alphabetically within each group)
  2. Namespace declaration
  3. Type declaration

---

## Naming Conventions

| Construct | Convention | Example |
|-----------|-----------|---------|
| Namespace, class, record, struct, enum | `PascalCase` | `OrderService`, `PaymentStatus` |
| Interface | `IPascalCase` | `IOrderRepository` |
| Public property / method / event | `PascalCase` | `PlaceOrderAsync`, `CustomerEmail` |
| Private / protected field | `_camelCase` | `_orderRepository` |
| Local variable / parameter | `camelCase` | `orderId`, `cancellationToken` |
| Constant (`const` / `static readonly`) | `PascalCase` | `MaxRetryCount` |
| Generic type parameter | `T` prefix | `TEntity`, `TResult` |
| Async method | `Async` suffix | `GetOrderAsync` |

---

## `var` Keyword

- Use `var` when the **type is obvious** from the right-hand side (object creation, cast, LINQ).
- Use the **explicit type** when the type is not immediately clear or when clarity aids the reader.

```csharp
// Good — type is obvious
var order = new Order();
var orders = await _repo.GetAllAsync(ct);

// Good — explicit when type is not obvious
IEnumerable<Order> results = GetFilteredOrders(criteria);
```

---

## Modern C# Features

Apply the following modern language features where they improve clarity:

### Primary Constructors (C# 12)
```csharp
public class OrderService(IOrderRepository repo, IEmailSender email) : IOrderService
{
    public async Task PlaceOrderAsync(Order order, CancellationToken ct = default)
    {
        order.Validate();
        await repo.SaveAsync(order, ct);
        await email.SendConfirmationAsync(order.CustomerEmail, ct);
    }
}
```

### Records for Immutable Data
```csharp
public record OrderSummary(Guid Id, string CustomerEmail, decimal Total, DateTimeOffset PlacedAt);
```

### Pattern Matching
```csharp
// Switch expression
string Describe(Shape shape) => shape switch
{
    Circle c   => $"Circle with radius {c.Radius}",
    Rectangle r => $"Rectangle {r.Width}×{r.Height}",
    _           => "Unknown shape"
};

// Property pattern
if (order is { Status: OrderStatus.Pending, Total: > 1000m })
    ApplyHighValueDiscount(order);
```

### Expression-Bodied Members
Use for simple, single-expression implementations:
```csharp
public string FullName => $"{FirstName} {LastName}";
public override string ToString() => $"Order({Id})";
```

### Collection Expressions (C# 12)
```csharp
string[] names = ["Alice", "Bob", "Carol"];
List<int> ids = [1, 2, 3];
```

---

## Null Handling

- Enable `<Nullable>enable</Nullable>` in every `.csproj` file.
- Use the **null-conditional operator** (`?.`) and **null-coalescing operator** (`??`) to write concise null-safe code.
- Use `ArgumentNullException.ThrowIfNull()` to guard public API entry points.
- Use the `is null` / `is not null` patterns rather than `== null` / `!= null` in new code.

```csharp
// Guard
public void SetCustomer(Customer? customer)
{
    ArgumentNullException.ThrowIfNull(customer);
    _customer = customer;
}

// Null-conditional + coalescing
var city = order?.ShippingAddress?.City ?? "Unknown";
```

---

## LINQ

- Prefer **method syntax** over query syntax for short chains; use query syntax for complex multi-clause queries that read more naturally.
- Avoid materialising a sequence (`.ToList()`, `.ToArray()`) unless you need random access, multiple enumeration, or to avoid deferred execution side-effects.
- Never use LINQ inside a tight loop if it causes repeated heap allocations — prefer imperative loops in perf-sensitive paths.

---

## Accessibility Modifiers

- Be **explicit** about all access modifiers; do not rely on defaults.
- Default to the **most restrictive** modifier possible: `private` for fields, `internal` for types not part of a public API.
- Mark classes that are not designed for inheritance as `sealed`.

---

## Miscellaneous

- Prefer `string.IsNullOrWhiteSpace` over `string.IsNullOrEmpty` unless trailing/leading whitespace is intentionally meaningful.
- Use `nameof()` wherever a member name must appear as a string (logging, exceptions, data annotations).
- Prefer `DateTimeOffset` over `DateTime` for timestamps; avoid `DateTime.Now` in favour of `DateTime.UtcNow` or an injected `IDateTimeProvider`.
- Use `Guid.NewGuid()` or `TimeProvider`-sourced IDs for entity identifiers unless the project uses a specific ID generation strategy.
