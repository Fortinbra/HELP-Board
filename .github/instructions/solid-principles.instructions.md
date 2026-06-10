---
applyTo: "**/*.cs"
---

# SOLID Principles for .NET / C#

Apply the following SOLID principles to every class, interface, and method you write or modify.

---

## Single Responsibility Principle (SRP)

A class should have **one, and only one, reason to change**. Each class encapsulates a single concern.

**Do:**
```csharp
// Handles only order persistence
public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task SaveAsync(Order order, CancellationToken ct = default)
        => await db.Orders.AddAsync(order, ct);
}

// Handles only order business rules
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

**Don't:** put database access, business logic, and email sending all in one class.

---

## Open/Closed Principle (OCP)

Code should be **open for extension but closed for modification**. Add new behaviour by creating new types, not by editing existing ones.

**Do:** use interfaces or abstract base classes so new variants can be added without touching existing code.
```csharp
public interface IDiscountStrategy
{
    decimal Apply(decimal price);
}

public class SeasonalDiscount : IDiscountStrategy
{
    public decimal Apply(decimal price) => price * 0.9m;
}

public class LoyaltyDiscount : IDiscountStrategy
{
    public decimal Apply(decimal price) => price * 0.85m;
}

// OrderPricer never changes when a new discount type is introduced
public class OrderPricer(IDiscountStrategy discount)
{
    public decimal Calculate(decimal basePrice) => discount.Apply(basePrice);
}
```

**Don't:** use `switch` / `if-else` chains that must be modified whenever a new variant is added.

---

## Liskov Substitution Principle (LSP)

A derived class must be **fully substitutable** for its base class without altering program correctness. Override only when the child type truly is a specialisation of the parent.

**Do:**
```csharp
public abstract class Shape
{
    public abstract double Area();
}

public class Rectangle(double width, double height) : Shape
{
    public override double Area() => width * height;
}

public class Circle(double radius) : Shape
{
    public override double Area() => Math.PI * radius * radius;
}
```

**Don't:** throw `NotImplementedException` in overrides, or weaken preconditions / strengthen postconditions in subtypes.

---

## Interface Segregation Principle (ISP)

Clients should not be forced to depend on interfaces they do not use. **Prefer many small, focused interfaces** over one large general-purpose interface.

**Do:**
```csharp
public interface IOrderReader
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

public interface IOrderWriter
{
    Task SaveAsync(Order order, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

**Don't:** create a single `IOrderRepository` with ten methods and force read-only services to depend on write methods they never call.

---

## Dependency Inversion Principle (DIP)

High-level modules should not depend on low-level modules. **Both should depend on abstractions.** Depend on interfaces, not concrete classes, and inject dependencies through the constructor.

**Do:**
```csharp
// High-level module depends on abstraction
public class ReportGenerator(IOrderReader orders, IReportFormatter formatter)
{
    public async Task<string> GenerateAsync(DateRange range, CancellationToken ct = default)
    {
        var orders = await orders.GetByDateRangeAsync(range, ct);
        return formatter.Format(orders);
    }
}
```

Registration in the DI container:
```csharp
services.AddScoped<IOrderReader, SqlOrderRepository>();
services.AddScoped<IReportFormatter, CsvReportFormatter>();
services.AddScoped<ReportGenerator>();
```

**Don't:** call `new SqlOrderRepository()` inside a service class; this hard-couples the high-level policy to the low-level detail.

---

## General Guidance

- When a class needs more than **three or four constructor parameters**, treat it as a signal that it may be violating SRP — consider splitting responsibilities.
- Prefer **composition over inheritance**; inherit from a concrete class only when there is a genuine IS-A relationship.
- Mark classes that are not designed for inheritance as `sealed`.
