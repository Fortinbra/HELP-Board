# PostgreSQL and EF Core

HELP-Board uses PostgreSQL on PiDB through the scoped `AppDbContext` and
repositories. The product owner reported that PiDB is working in production on
2026-11-18. This report does not establish that backups, recovery, production
configuration, migrations, or PostgreSQL-specific tests have been verified.
Outstanding work is tracked in the [feature backlog](features/backlog.md#persistence).

## Connection and data model

The API reads `ConnectionStrings:HelpBoard`, supplied to the container as
`ConnectionStrings__HelpBoard` through `DB_CONNECTION_STRING` in Compose. Keep
credentials outside source control and use a least-privilege database user.
See the [deployment guide](deployment.md) for PiDB connectivity, TLS, and
runtime configuration. Pool, timeout, and lifetime settings must be validated
against PiTony/PiDB capacity; example values are not production benchmarks.

The current proof of concept stores Tickets with GUID keys, UTC timestamps,
string-converted status and priority, required fields and length limits. The
initial EF migration creates the `Tickets` table and indexes on `CreatedAt`
and `Status`. Future boards and strategic work items belong to their respective
features; the Director's Report requires its own data model.

## Migration workflow

Migration files and the model snapshot live in
`src/HelpBoard.Repositories/Migrations`. Use an EF CLI version compatible with
the project's EF Core packages. The commands below run from the repository root;
the API is the startup project and must receive a valid connection string. Set
`FeatureManagement__DatabaseMigrationsApplyAtStartup=false` before running
design-time commands so startup does not apply migrations implicitly. Confirm
the selected connection and environment before any database update.

Create and review a new migration after changing the model:

```powershell
dotnet ef migrations add MyMigrationName --project src\HelpBoard.Repositories --startup-project src\HelpBoard.Api
```

For an explicitly selected non-production database, apply reviewed migrations:

```powershell
dotnet ef database update --project src\HelpBoard.Repositories --startup-project src\HelpBoard.Api
```

Generate a script for review before any approved production application:

```powershell
dotnet ef migrations script --idempotent --project src\HelpBoard.Repositories --startup-project src\HelpBoard.Api --output .\migration.sql
```

Treat generated scripts as operational artifacts; review and protect them as
appropriate rather than committing them by default. Production startup
migrations remain disabled. Review migration history, verify a usable backup,
and approve the target, execution, and recovery plan before applying changes.
The detailed production execution and backup/restore procedures remain
outstanding.

EF can target a previous migration with `dotnet ef database update` and the
previous migration's name, but a down migration may destroy data. In
particular, the initial migration's `Down` drops the `Tickets` table. Do not
assume an image rollback reverses a database migration or restores data; test
supported recovery paths on an isolated database before use. Never modify or
delete deployed migration history; create a new reviewed migration instead.

## Transactions and validation

Each repository write explicitly calls `SaveChangesAsync`; EF normally wraps
that save in a transaction. Coordinated multi-repository writes require an
explicit transaction on the shared scoped context, with commit on success and
rollback on failure. Keep transactions short and propagate cancellation.

Request validation belongs at the API boundary, business and cross-aggregate
rules in services, and relational constraints in database mappings. Current
in-memory and SQLite tests do not establish PostgreSQL constraint, transaction,
or migration behavior; real PostgreSQL verification remains outstanding.
