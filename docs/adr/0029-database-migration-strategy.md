# ADR-0029: Database migration strategy for .NET (EF Core)
- Status: Proposed
- Date: 2026-03-31
- By: Usman Rafiq
- Related: ADR-0002 (repository structure), ADR-0011 (module boundaries), ADR-0026 (outbox pattern)
### The problem

The most common migration failure modes:

- **Downtime**: migration locks tables for too long, blocking all queries
- **Data loss risk**: column deletions or type changes that lose data
- **Backward incompatibility**: new app code expects new columns that old code doesn't have at same time
- **Multi-tenant DBs**: shared schema across tenants; a failed migration impacts all tenants
- **No rollback**: migration is one-way; production data must be manually fixed

### Constraints

- Migrations must work for both **development** and **production**
- Must support **zero-downtime deployments** (blue-green, canary)
- Must be **idempotent** where possible
- Must work with **EF Core** (the chosen ORM)
- Must be **auditable** (each migration has a version, checksum, and description)

## Decision

We will use **EF Core Migrations** with a strict, documented workflow:

### Migration principles

| Principle | Description |
|-----------|-------------|
| **Always use migrations from code** | Never manually edit the database; all changes go through `dotnet ef migrations add` |
| **Always run in production via CI/CD** | Never manually run migrations on production; they are part of the deploy pipeline |
| **Timelessness** | Migrations must be safe to run at any time (idempotent, reversible where possible) |
| **Test in staging** | Migrations are exercised against a staging environment before production |
| **No manual schema changes** | All changes must pass through the migration pipeline |

### Migration workflow

```bash
# 1. Create a migration
dotnet ef migrations add AddColumn_TenantApiKey_AdminToken --project src/Infrastructure/AccountingDbContext

# 2. Review the generated migration file
# (check for expensive operations, long locks, etc.)

# 3. Build the Docker image
docker build -t myapi:v1.2.3 .

### Example: safe two-step migration

```csharp
// Migration step 1: Add new nullable column
public partial class AddColumn_NewField_Nullable : Migration
{
    protected override void Up(MigrationBuilder mb)
    {
        mb.AddColumn<string>("MyTable", "NewField", nullable: true);
        // Backfill data...
    }

    protected override void Down(MigrationBuilder mb)
    {
        mb.DropColumn("MyTable", "NewField");
    }
}

// Migration step 2: After app code is deployed, make it non-nullable
public partial class MakeField_NonNullable : Migration
{
    protected override void Up(MigrationBuilder mb)
    {
        // Data already exists from step 1; nothing to backfill
        mb.AlterColumn<string>("MyTable", "NewField", nullable: false);
    }

    protected override void Down(MigrationBuilder mb)
    {
        mb.AlterColumn<string>("MyTable", "NewField", nullable: true);
    }
}
```

## Alternatives considered

### Option A: No migrations (manual SQL scripts)

- **Pros**: Direct control
- **Cons**: No versioning, no rollback, high risk of inconsistency
- **Verdict**: Rejected (no audit trail)

### Option B: Flyway / Liquibase (separate DB migration tools)

- **Pros**: Language-agnostic; database-specific features; good for polyglot
- **Cons**: Additional infrastructure; steeper learning curve
- **Verdict**: Acceptable alternative; EF Core chosen for simplicity in .NET ecosystems

### Option C: Code-based migrations with `dotnet ef migrations add`

- **Pros**: Version-controlled, peer-reviewable, testable, .NET-native
- **Cons**: Requires learning EF Core migration API
- **Verdict**: Chosen

### Option D: Auto-migrate at startup (no explicit migration management)

- **Pros**: Simple
- **Cons**: Migrations run in production without review; high risk of broken deploys
- **Verdict**: Rejected (automation is good, but only with review gates)

## Consequences

### ✅ Good

- **Reproducible migrations** — every environment gets the same schema
- **Audit trail** — every migration has a version, author, and description
- **Zero-downtime deployments** — safe migration patterns avoid table locks
- **Rollback is possible** — `dotnet ef database update <previous-version>`
- **Review gate** — migrations are code-review and test-gated before deployment

### ❌ Bad

- **Complex multi-step migrations** can still cause downtime (e.g., changing column types)
- **Large tables** (10M+ rows) can make even nullable column additions slow
- **Migration files grow** — must be kept lean and reviewed
- **Staging environment may differ** from production (different SQL dialect, indexes)

### 🔧 Follow-ups

- Add **migration review** as a mandatory PR checklist item
- Run migrations on **staging** before production
- Monitor `ALTER TABLE` duration in production
- Add **pre-deployment dry run** in CI
- Document **migration runbook** in the repository (including rollback procedures)

## References

- ADR-0002: How this repository is structured
- ADR-0011: Module boundary rules
- ADR-0026: Outbox pattern (event persistence)

## Architect Note

> A database migration is a **production change** first, a code change second.  
> If you can't explain the migration risk to the on-call engineer, you haven't thought about it enough.

# 4. Push to registry
docker push myapi:v1.2.3

# 5. Deploy with migration in the startup pipeline
# (EF Core runs migrations before the app starts)
```

### Startup migration pattern (EF Core)

```csharp
// Program.cs - EF Core auto-migration
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    context.Database.Migrate(); // Runs pending migrations before serving requests
}
```

### Safe migration patterns

| Pattern | Description |
|---------|-------------|
| **Add column (nullable)** | Safe - no downtime, no data loss |
| **Add column (non-nullable)** | Requires a default value + backfill script; must be done as 2 steps |
| **Drop column** | Two-phase: (1) deprecate in code, (2) after audit period, drop in a separate deploy |
| **Add index** | Always use `CREATE INDEX CONCURRENTLY` (Postgres) or `WITH (ONLINE = ON)` (SQL Server) |
| **Change column type** | Two-phase: (1) add new column, (2) backfill, (3) switch reads/writes, (4) drop old |


## Context

Database migrations are one of the most operationally critical decisions for a .NET application:
