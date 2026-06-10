---
applyTo: "**/*{Test,Tests,Spec,Specs}.cs"
---

# Test-Driven Development (TDD) Practices for .NET / C#

These guidelines apply to all test files and govern how tests should be written, structured, and maintained across any .NET project.

---

## Red → Green → Refactor

Always follow the TDD cycle:

1. **Red** — Write a failing test that describes the next small piece of behaviour.
2. **Green** — Write the simplest production code that makes the test pass. Resist over-engineering at this stage.
3. **Refactor** — Clean up both the production code and the test without changing observable behaviour. Re-run the tests after every refactoring step.

---

## Test Naming Convention

Use the `MethodName_Scenario_ExpectedBehavior` (or `MethodName_StateUnderTest_ExpectedBehavior`) convention so that a failing test name is self-explanatory without reading the test body.

```csharp
// Good test names
[Fact]
public void PlaceOrder_WithOutOfStockItem_ThrowsOutOfStockException() { }

[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsOrder() { }

[Fact]
public async Task GetByIdAsync_WithUnknownId_ReturnsNull() { }
```

---

## Arrange / Act / Assert (AAA)

Structure every test with three clearly separated sections, using a **blank line** between each:

```csharp
[Fact]
public async Task PlaceOrderAsync_ValidOrder_SavesOrderAndSendsEmail()
{
    // Arrange
    var order = new Order { CustomerEmail = "test@example.com" };
    var repoMock = new Mock<IOrderRepository>();
    var emailMock = new Mock<IEmailSender>();
    var sut = new OrderService(repoMock.Object, emailMock.Object);

    // Act
    await sut.PlaceOrderAsync(order);

    // Assert
    repoMock.Verify(r => r.SaveAsync(order, default), Times.Once);
    emailMock.Verify(e => e.SendConfirmationAsync(order.CustomerEmail, default), Times.Once);
}
```

---

## Test Isolation

- Each test must be **fully independent** — it must not rely on the state left by another test.
- Mock all external dependencies (databases, file systems, HTTP services, clocks) using interfaces.
- Never use `static` mutable state in tests.
- Reset all mocks between tests; prefer creating a fresh instance per test rather than sharing class-level instances unless using a test fixture.

---

## Test Doubles

Use the right type of test double for each situation:

| Double | When to use |
|--------|------------|
| **Stub** | Return a canned value; used to satisfy a dependency. |
| **Mock** | Verify that a specific method was called with specific arguments. |
| **Fake** | A lightweight working implementation (e.g., in-memory repository). |
| **Spy** | Wrap a real object to record calls while still invoking real behaviour. |

Prefer **Moq** (or **NSubstitute**) for mocking in unit tests; prefer in-memory implementations or `WebApplicationFactory` for integration tests.

---

## Test Granularity

| Layer | Test type | Rule |
|-------|-----------|------|
| Domain logic / services | Unit test | Mock all dependencies; no I/O. |
| Repository / data access | Integration test | Use a real (in-memory or test-container) database. |
| HTTP API endpoints | Integration test | Use `WebApplicationFactory<TProgram>`. |
| End-to-end flows | E2E test | Sparingly; cover only the most critical paths. |

---

## Data-Driven Tests

Use `[Theory]` with `[InlineData]`, `[MemberData]`, or `[ClassData]` to avoid duplicating test structure:

```csharp
[Theory]
[InlineData("", false)]
[InlineData(" ", false)]
[InlineData(null, false)]
[InlineData("valid@email.com", true)]
public void IsValidEmail_VariousInputs_ReturnsExpected(string? email, bool expected)
{
    // Arrange & Act
    var result = EmailValidator.IsValid(email);

    // Assert
    Assert.Equal(expected, result);
}
```

---

## Test Coverage

- Prioritise **coverage of business logic and domain rules** over mechanical CRUD code.
- Test **edge cases**: null inputs, empty collections, boundary values, and failure paths.
- Do not write tests purely to hit a coverage metric — a poorly written passing test is worse than no test.
- Tests are production code: keep them **clean, readable, and refactored**.

---

## Test Project Organisation

- Mirror the production project structure: one test class per production class.
- Name the test project `<ProjectName>.Tests` (unit) or `<ProjectName>.IntegrationTests`.
- Use a shared `TestFixture` or `WebApplicationFactory` for expensive setup that is genuinely shared across tests, but document that it is intentionally shared.
