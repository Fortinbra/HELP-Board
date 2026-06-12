# Feature 009: PostgreSQL & EF Core Integration

## Goal

Establish a persistent data layer using Entity Framework Core and PostgreSQL that supports all HelpBoard domain entities (Ticket, ProjectBoard, WorkItem, Strategic Plan items) with production-ready migrations infrastructure, connection pooling, transaction handling, and repository pattern implementation.

## User Value

Users benefit from durable data persistence across application restarts and deployments. Data integrity is guaranteed through transactional consistency. Teams can confidently track work progress and strategic plans in a production-grade relational store. Operations teams can manage migrations, backups, and scaling confidently with PostgreSQL.

## Scope

### In Scope

- Establish and document `AppDbContext` configuration for Ticket entity (proof of concept; foundation for future entities).
- Define PostgreSQL connection string infrastructure with PiDB server reference and environment-specific configuration.
- Create and document initial EF Core migration (`001_InitialCreate`) that creates the Ticket table schema.
- Establish migrations infrastructure pattern and document the manual migration workflow.
- Implement repository pattern for Ticket and ProjectBoard domains (placeholder for ProjectBoard until Feature 004 defines the entity).
- Wire `AddRepositories()` dependency injection for DbContext and repository registration.
- Establish connection pooling and transaction handling best practices in documentation.
- Define data validation rules at DbContext configuration level (property length, constraints, conversions).
- Write persistence roundtrip tests demonstrating Ticket and ProjectBoard repository behavior.
- Document PostgreSQL connection string format and PiDB server setup requirements.
- Document the migrations workflow (creation, application, rollback scenarios).

### Out of Scope

- Advanced migrations scenarios (data seeding, conditional migrations, complex snapshots).
- Query optimization and indexing strategy beyond initial schema constraints.
- Read replicas, sharding, or advanced PostgreSQL deployment patterns.
- Full-text search or JSON/JSONB advanced PostgreSQL features.
- Change tracking audit trails or soft-delete patterns (deferred to future feature).
- High-availability or disaster-recovery strategies beyond connection string documentation.

## Data Needs

### Data Sources

- HelpBoard domain entities: Ticket, ProjectBoard (to be defined by Feature 004), WorkItem (to be defined by Feature 004), Strategic items (objectives, initiatives, milestones; to be defined by Feature 003).
- PostgreSQL PiDB server with database creation and access credentials.
- Connection string managed via configuration (`appsettings.json`, environment variables).

### Data Model Considerations

- **Ticket Entity** (proof of concept; already implemented):
  - Primary key: `Id` (GUID).
  - Required fields: `Title` (max 200 chars), `Description` (max 4000 chars), `CreatedBy` (max 100 chars).
  - Status and Priority enums stored as strings via value conversion.
  - Timestamps: `CreatedAt`, `UpdatedAt` (DateTimeOffset, UTC).
  - No soft delete yet; deletions are permanent.

- **Future Entities** (Feature 003, 004):
  - `ProjectBoard`: `Id`, `Name`, `Description`, board lifecycle metadata, non-deletable constraints for Strategic Plan system board.
  - `WorkItem`: `Id`, parent board foreign key, `Title`, `Description`, status, priority, owner, due date, progress percentage.
  - `Strategic*`: Objective, Initiative, Milestone entities with hierarchy IDs, owner attribution, progress roll-up fields.

- **Shared Persistence Concerns**:
  - All entities use GUID primary keys for distributed generation.
  - All entities include ownership/accountability metadata (`CreatedBy`, `UpdatedBy` timestamps).
  - Enums are stored as strings for database readability and migration flexibility.
  - Foreign key constraints enforce referential integrity.
  - No circular dependencies or complex cascade deletes without explicit documentation.

### Data Freshness

- Immediate persistence for all writes (no eventual consistency at this layer).
- Reads use EF Core tracking/no-tracking as appropriate for query isolation.
- Historical snapshots deferred to future reporting/audit features.

## Database Infrastructure

### Connection Configuration

- **Environment Variable**: `ConnectionStrings__HelpBoard` (colon-delimited in appsettings.json becomes double underscore in environment).
- **Connection String Format**:
  ```
  Host=<PiDB_HOST>;Port=5432;Database=helpboard_<env>;Username=<USER>;Password=<PASSWORD>;
  ```
- **Timeout and Pooling**:
  - Connection pooling defaults managed by Npgsql (min 10, max 30 connections).
  - Command timeout: 30 seconds (configurable per query if needed).
  - Connection lifetime: 5 minutes (configurable in advanced scenarios).

### PostgreSQL Setup (PiDB)

- Database name: `helpboard_dev` (development), `helpboard_staging`, `helpboard_prod`.
- Create database with UTF-8 encoding.
- Create application user with limited privileges (not superuser).
- All migrations are applied by the application at startup or via CLI tooling.

## Persistence Layer Architecture

### AppDbContext

- Located: `src/HelpBoard.Repositories/Data/AppDbContext.cs`.
- Responsibilities:
  - DbSet properties for all tracked entities.
  - OnModelCreating configuration (entity mappings, property constraints, conversions, indexes).
  - No query logic; data access is delegated to repositories.
  - Sealed class; no inheritance chains.
- Lifetime: Scoped per HTTP request (one DbContext instance per unit of work).

### Repository Pattern

- **Interfaces** (contracts in `HelpBoard.Abstractions.Repositories`):
  - `ITicketReader`: async read operations (GetByIdAsync, GetAllAsync, etc.).
  - `ITicketWriter`: async write operations (AddAsync, UpdateAsync, DeleteAsync).
  - `IProjectBoardReader`, `IProjectBoardWriter`: placeholder for Feature 004.
  - `IStrategicReader`, `IStrategicWriter`: placeholder for Feature 003.

- **Implementations** (in `HelpBoard.Repositories/<Domain>/`):
  - `TicketRepository(AppDbContext dbContext)`: implements ITicketReader + ITicketWriter.
  - `ProjectBoardRepository(AppDbContext dbContext)`: placeholder for Feature 004.
  - Each repository is responsible for one aggregate root and its immediate children.

- **Constraints**:
  - All operations are async with CancellationToken support.
  - No repository-level caching; caching is handled by services or HTTP layers.
  - Change tracking is managed explicitly (AsNoTracking for reads unless update is needed).
  - SaveChangesAsync is called explicitly at the end of each write operation.

### Dependency Injection

- `AddRepositories(IServiceCollection, connectionString)` extension method in `HelpBoard.Repositories/DependencyInjection.cs`:
  - Registers `AppDbContext` as Scoped with Npgsql provider.
  - Registers each repository interface + implementation as Scoped.
  - Throws InvalidOperationException if connection string is missing.
- **Usage in Program.cs**:
  ```csharp
  var connectionString = builder.Configuration.GetConnectionString("HelpBoard")
      ?? throw new InvalidOperationException("Connection string 'HelpBoard' is not configured.");
  builder.Services.AddRepositories(connectionString);
  ```

## EF Core Migrations Infrastructure

### Initial Migration

- **Name**: `001_InitialCreate`
- **Contents**:
  - Create `tickets` table with Ticket entity schema.
  - Define primary key, required columns, length constraints, indexes on frequently queried columns (e.g., `CreatedAt`, `Status`).
  - Define character set and collation (UTF-8).
  - Placeholder comments for future migrations (ProjectBoard, WorkItem, Strategic entities).

- **Generated by**:
  ```bash
  dotnet ef migrations add InitialCreate --project src/HelpBoard.Repositories --startup-project src/HelpBoard.Api
  ```

### Migrations Folder Structure

- Located: `src/HelpBoard.Repositories/Migrations/`.
- Contents:
  - `<timestamp>_<MigrationName>.cs`: migration logic (Up/Down).
  - `<timestamp>_<MigrationName>.Designer.cs`: EF Core-generated snapshot metadata.
  - `AppDbContextModelSnapshot.cs`: current model snapshot for diff calculations.

### Migration Workflow

- **Apply to Development**:
  ```bash
  dotnet ef database update --project src/HelpBoard.Repositories --startup-project src/HelpBoard.Api
  ```
  - If `appsettings.Development.json` defines a local PiDB connection, migrations apply to that database.
  - Idempotent: safe to run multiple times.

- **Apply to Staging/Production**:
  - Generated SQL script via `dotnet ef migrations script` for manual review and scheduled application.
  - Applied by database administrator or deployment automation (not by application startup in production).

- **Rollback** (if needed):
  - `dotnet ef database update <previous-migration>` to roll back to a prior state.
  - Down methods in migration files are tested; down-and-up cycles must preserve data semantics.

### Migrations Governance

- All schema changes require a migration file (never manual ALTER TABLE).
- Migration files are code-reviewed and tested before merge.
- Migration files are NOT deleted; history is immutable.
- If a breaking change is needed in a deployed environment, a new migration is created (not destructive).
- Seed data migrations are avoided unless absolutely necessary; data setup is handled by application initialization logic, not migrations.

## Connection Pooling & Transaction Handling

### Connection Pooling

- Npgsql connection pooling is enabled by default (min 10, max 30).
- Configuration via connection string parameter `Pooling=true; Min Pool Size=10; Max Pool Size=30;`.
- Connection reuse is transparent to application code.

### Transaction Handling

- **Default Behavior**:
  - SaveChangesAsync wraps changes in a database transaction automatically.
  - One transaction per unit of work (one repository operation per transaction).

- **Explicit Transactions** (if needed for multi-repository operations):
  ```csharp
  using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
  try {
      // Multiple operations
      await repository1.UpdateAsync(...);
      await repository2.UpdateAsync(...);
      await transaction.CommitAsync(cancellationToken);
  } catch {
      await transaction.RollbackAsync(cancellationToken);
      throw;
  }
  ```

- **Best Practice**: Keep transactions short and focused; avoid user interaction within a transaction.

## Data Validation

### Entity-Level Constraints

- **Ticket Entity** (example):
  - `Title`: Required, max 200 characters, non-null.
  - `Description`: Required, max 4000 characters, non-null.
  - `CreatedBy`: Required, max 100 characters, non-null.
  - `Status`, `Priority`: Enums stored as string; validation at DbContext configuration level via HasConversion<string>.

### Database-Level Constraints

- Primary keys (NOT NULL, UNIQUE).
- Foreign keys (referential integrity, cascade delete only if semantically safe).
- Check constraints for enum/status ranges (optional; can be applied via trigger if needed).
- Indexes on frequently queried columns (CreatedAt, Status, UpdatedBy).

### Validation Boundary

- **In DbContext**: Property-level constraints (length, required, type conversions).
- **In Repository**: Business rule validation (e.g., "ticket status transition from Open to Closed is only valid if all tasks are done").
- **In Service**: Cross-aggregate validation and coordination logic.
- **In API Controller**: Request-level validation (FluentValidation or DataAnnotations).

## Accessibility (WCAG)

Not applicable to this infrastructure feature. Accessibility concerns are addressed by UI and API response layers in dependent features.

## Testing Strategy

### Unit Tests (InMemoryDatabase)

- Location: `tests/HelpBoard.Repositories.Tests/`.
- Pattern:
  - Each repository has a test class (e.g., `TicketRepositoryTests`).
  - Use `DbContextOptionsBuilder<AppDbContext>.UseInMemoryDatabase()` for isolation.
  - Test CRUD roundtrips: create, retrieve, update, delete.
  - Test query filters and sorting.
  - Test error conditions (not found, duplicate keys, constraint violations).

- **Example Test** (Ticket repository roundtrip):
  ```csharp
  [Fact]
  public async Task AddAsync_WithValidTicket_PersistsAndRetrieves()
  {
      // Arrange
      var ticket = new Ticket {
          Title = "Test",
          Description = "Description",
          CreatedBy = "user@example.com"
      };

      // Act
      await repository.AddAsync(ticket);
      var retrieved = await repository.GetByIdAsync(ticket.Id);

      // Assert
      Assert.NotNull(retrieved);
      Assert.Equal(ticket.Title, retrieved.Title);
  }
  ```

### Integration Tests (PostgreSQL)

- **Not in scope for Feature 009** but planned for future.
- Would use a test PostgreSQL container (Docker) or test database instance.
- Would test against actual schema, indexes, and query performance.
- Would validate migration application and rollback scenarios.

## Dependencies

- **Feature 001**: UI Foundation & Theme System (infrastructure independent; no UI in this feature).
- **Feature 003**: Strategic Plan Tracker (defines Strategic entity models that will be added to DbContext).
- **Feature 004**: Project Boards (defines ProjectBoard and WorkItem entity models that will be added to DbContext).
- **External**: PostgreSQL server (PiDB) for development/staging/production data storage.

## Risks

### High Priority

1. **Migration Strategy Complexity**
   - Risk: Migrations applied out of order or manually by different teams, causing schema drift.
   - Mitigation:
     - Migrations are versioned and applied sequentially; skip versioning is not allowed.
     - All migrations are committed to source control and code-reviewed.
     - Production migrations are applied via automation or explicit approval workflow, not ad hoc.

2. **Data Validation Gaps**
   - Risk: Invalid data persisted to database if entity-level or repository-level validation is incomplete.
   - Mitigation:
     - DbContext constraints are explicit (required fields, max length).
     - Repository methods include business rule validation before SaveChangesAsync.
     - API controllers use FluentValidation or DataAnnotations for request-level validation.
     - Tests validate both happy path and error conditions.

3. **Connection Pooling Exhaustion**
   - Risk: Slow queries or long-lived DbContext instances exhaust the connection pool, causing application hangs.
   - Mitigation:
     - DbContext is Scoped (one per request, disposed at request end).
     - Queries use async/await patterns and respect CancellationToken.
     - Connection string sets Max Pool Size to 30 (sufficient for typical load).
     - Monitoring alerts on connection pool utilization.

### Medium Priority

4. **Transaction Deadlocks**
   - Risk: Concurrent writes to the same entity cause lock timeouts or deadlocks.
   - Mitigation:
     - Default SaveChangesAsync transaction isolation handles most cases.
     - Explicit transactions are minimized and documented.
     - Critical paths have deadlock retry logic (if needed).

5. **Migrations Require Downtime**
   - Risk: Large schema changes (adding non-nullable columns, dropping tables) require downtime or careful coordination.
   - Mitigation:
     - Breaking schema changes are avoided in first releases.
     - If unavoidable, migrations are coordinated with deployment strategy (blue-green deployment, maintenance windows).
     - Documentation includes pre-migration checklist (backups, testing, rollback plan).

6. **Query Performance Regression**
   - Risk: Unoptimized queries cause slow API responses or high database load.
   - Mitigation:
     - EF Core query composition is tested; complex queries are reviewed.
     - Frequently accessed queries have indexes defined in OnModelCreating.
     - Query performance is monitored in staging/production.

### Low Priority

7. **DbContext Async Over-Complication**
   - Risk: Developers misuse async patterns (sync-over-async, improper CancellationToken handling).
   - Mitigation:
     - Repository interfaces define all methods as async.
     - Copilot instruction enforces CancellationToken propagation and no .Result/.Wait() usage.

## Copilot Governance

### Candidate Instructions

- **EF Core DbContext Configuration**:
  - All entities must be configured in OnModelCreating with explicit property constraints (required, max length, conversions).
  - No shadow properties; all mapped properties must be explicit in entity classes.
  - No query logic in DbContext; queries are delegated to repositories.
  - DbContext must be sealed and have no inheritance.

- **Repository Pattern**:
  - Each repository implements segregated reader/writer interfaces (IXyzReader, IXyzWriter).
  - All operations are async with CancellationToken support.
  - No async-over-sync patterns; CancellationToken must propagate through entire call chain.
  - SaveChangesAsync is called explicitly at the end of each write operation.
  - Queries use AsNoTracking for read-only operations; change tracking is explicit.

- **Migrations**:
  - All schema changes require a new migration (no manual ALTER TABLE).
  - Migration files are code-reviewed before merge and never deleted.
  - Down/Up methods must be symmetric and preserve data semantics.
  - Seed data is avoided in migrations; use application initialization instead.

- **Connection & Transaction Handling**:
  - DbContext is Scoped (one per HTTP request).
  - Explicit transactions are minimized and wrapped in try-catch-finally blocks with proper rollback.
  - Connection strings are managed via configuration; no hardcoded credentials.

### Candidate Skills

- **EF Core Configuration Review**: Verify OnModelCreating configuration for completeness, constraint correctness, and query-ability.
- **Repository Pattern Review**: Verify segregated interfaces, async patterns, SaveChangesAsync placement, and CancellationToken propagation.
- **Migration Safety Review**: Verify migration files for data-semantic correctness, reversibility, and impact analysis (new columns, dropped columns, constraint changes).
- **Performance Review**: Verify query composition, index coverage, and async patterns for connection pool safety.

### Reusable Prompts

- "Review this EF Core entity mapping for property constraints, conversions, and query-ability completeness."
- "Verify this repository method for async/await correctness, CancellationToken propagation, and SaveChangesAsync placement."
- "List the data migration risks (breaking changes, constraint violations, backfill requirements) for this schema migration."
- "Identify indexes or query patterns needed to support this new repository query efficiently."

## Acceptance Criteria

- [ ] **AppDbContext Configuration**: `AppDbContext` is defined with DbSet<Ticket> and explicit OnModelCreating configuration for property constraints, conversions, and required fields.
- [ ] **Ticket Repository Proof of Concept**: `TicketRepository` implements both `ITicketReader` and `ITicketWriter` interfaces with full CRUD operations, all async with CancellationToken support.
- [ ] **DependencyInjection Wiring**: `AddRepositories(IServiceCollection, connectionString)` registers AppDbContext as Scoped with Npgsql provider; all repositories are registered as Scoped.
- [ ] **PostgreSQL Connection Infrastructure**: Connection string configuration via `appsettings.json` and environment variables is documented; PiDB server setup requirements (database creation, user credentials) are documented.
- [ ] **Initial Migration**: `001_InitialCreate` migration creates the `tickets` table with all Ticket entity columns, constraints, indexes, and is reversible.
- [ ] **Migrations Workflow Documentation**: Documentation covers migration creation (`dotnet ef migrations add`), application (`dotnet ef database update`), rollback, and script generation for production use.
- [ ] **Connection Pooling & Transaction Handling**: Documentation defines connection pool configuration, transaction lifecycle, and explicit transaction patterns with code examples.
- [ ] **Data Validation Documentation**: DbContext, repository, and API controller validation boundaries are defined; property constraints are documented.
- [ ] **Persistence Roundtrip Tests**: Unit tests for Ticket repository demonstrate full CRUD roundtrips with valid and invalid data (not found, constraint violations).
- [ ] **Placeholder for Feature 003/004 Entities**: DbContext is architected to accommodate future ProjectBoard, WorkItem, and Strategic entity models; comments indicate integration points.
- [ ] **Copilot Governance Artifacts**: Instructions and skills for EF Core configuration, repository pattern, migrations, and performance are documented.
- [ ] **No Breaking Changes**: All changes are backward compatible; existing in-memory test doubles continue to work.

## Open Questions

- Should feature-flag the initial migration application, or apply at startup by default?
- Should implement optimistic concurrency control (ConcurrencyToken/RowVersion) for conflict detection, or defer to future feature?
- Should connection pool and timeout settings be configurable per environment, or use defaults for MVP?

## Follow-Up Work (Post Feature 009)

- **Feature 003 Integration**: Define and add Strategic entity models (Objective, Initiative, Milestone) to DbContext; implement repository pattern for strategic queries and roll-up calculations.
- **Feature 004 Integration**: Define and add ProjectBoard and WorkItem entity models to DbContext; implement repository pattern for board lifecycle and work item queries.
- **Feature 007 Integration** (Feature Toggle System): Coordinate migration application with feature-toggle infrastructure; ensure migrations can be conditionally applied.
- **Advanced Persistence**: Implement query optimization, full-text search, audit trail tracking, soft deletes (if required by business logic).

---

This comprehensive feature document is ready to save as `docs/features/feature-009-postgres-efcore-integration.md`. It provides the Data Store Specialist with clear requirements for implementing the persistence infrastructure, while remaining grounded in the existing codebase context (Ticket entity, AppDbContext proof of concept, and test patterns already established).
