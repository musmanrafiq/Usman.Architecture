# ADR-0030: Saga handler single-purpose boundaries (read / write / HTTP)

- Status: Proposed
- Date: 2026-03-31
- By: Usman Rafiq
## Context

Saga handlers orchestrate distributed transactions across multiple services. Each handler steps through:
1. Read data from local or remote DB
2. Perform business logic
3. Write data back (local or remote)
4. Call another service over HTTP/REST

### The problem

When a single saga handler performs operations too eagerly, it can:

- Hold a database connection open while waiting for an external HTTP call to complete, **tying up the connection pool**
- Keep a write transaction open across multiple operations, **locking rows** and causing deadlocks
- Mix concerns — a handler that reads a cart, writes an order, and calls a payment gateway is hard to test, debug, or replay

### Symptoms of the problem

```csharp
// BAD: one handler does everything
public async Task Handle(PlaceOrderSaga cmd)
{
    // 1. Read from DB (database connection acquired)
    var cart = await _dbContext.Carts.FindAsync(cmd.CartId);

    // 2. Do business logic that may take time
    var total = CalculateTotal(cart);

    // 3. Write DB (connection still held!)
    await _dbContext.Orders.AddAsync(new Order { ... });
## Decision

We will enforce a **single-purpose restriction** on saga handlers:

| Handler type | Allowed operations | DB connection | HTTP calls | Logging |
|--------------|-------------------|---------------|------------|---------|
| **Read handler** | `SELECT`, projections, projections | Read-only transaction | ❌ No | ✅ Yes |
| **Write handler** | `INSERT`, `UPDATE`, `DELETE` + **outbox logging** | Write transaction (short) | ❌ No | ✅ Debug only |
| **HTTP handler** | No DB, no local writes | ❌ No (or very short) | ✅ Yes | ✅ Yes |
| **Finalize handler** | Outbox drain only | Short read-only | ❌ No | ✅ Info |

### Example: split handler into three

```csharp
// ListenForOrderPlaced — Read handler: just reads to decide
public class ListenForOrderPlaced : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent evt, CancellationToken ct)
    {
        // Read-only projection
        var product = await _db.Products.FindAsync(evt.ProductId);
        // No writes, no HTTP calls
        await _projectionStore.SaveAsync(evt, product.Price);
    }
}

// PublishToInventory — Write handler: persists state, logs to outbox
public class PublishToInventory : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent evt, CancellationToken ct)
    {
        // Write only — short transaction, no external calls
        await _db.Orders.AddAsync(new Order { ... });
        await _db.SaveChangesAsync(ct);

        // Log to outbox (ADR-0026)
## Alternatives considered

### Option A: Connection-pooling middleware (npgsql-waterbear, etc.)

- **Pros**: Handles shared connections, reuse across async boundaries
- **Cons**: Doesn't prevent long-running transactions; complexity; doesn't solve local DB connection limits
- **Verdict**: Rejected (still rely on request-level boundaries)

### Option B: DbContext pooling (Microsoft.Extensions.Options)

- **Pros**: Reuses DbContext instances across requests
- **Cons**: Doesn't solve connection execution time; more complex lifecycle
- **Verdict**: Rejected (doesn't address the core issue)

### Option C: Stop the world (no HTTP in saga handlers)

- **Pros**: Simple; all handlers are either DB read or DB write
- **Cons**: Loses flexibility; couples handlers to sync protocols
- **Verdict**: Rejected (some handlers legitimately need external calls)

### Option D: Enforce "read-only, write-only, HTTP-only" boundaries

- **Pros**: Simple rule; prevents connection pool exhaustion; short-lived transactions
- **Cons**: Requires discipline; may be too restrictive for some workflows
- **Verdict**: Chosen

## Consequences

### ✅ Good

- **Connection pool is protected** — no handler holds a connection during HTTP calls
- **Short-lived transactions** — less locking, fewer deadlocks
- **Testability** — each handler is atomic and can be unit-tested in isolation
- **Observable** — transaction duration is explicit in metrics
- **Safe to retry** — atomic handlers can be retried without side effects

### ❌ Bad

- **More handlers to write** — the same logic may be split across 3-4 handlers
- **Coordination overhead** — you need a coordinator to dispatch to the right handler
- **May be too strict** — some workflows genuinely need DB + HTTP in one step

### 🔧 Follow-ups

- Add a **lint rule** (e.g., in a Roslyn analyzer) that flags handlers doing both DB writes and HTTP calls
- Add **monitoring dashboards**: `saga_handler_duration_seconds`, `transaction_duration_seconds`
- Set up **alerting** when connection pool utilization exceeds 80%
- Document **anti-patterns** in code review checklists

## References

- ADR-0026: Outbox pattern for reliable event publishing
- ADR-0027: Observability with OpenTelemetry
- ADR-0029: Database migration strategy

## Architect Note

> A database connection is a **scarce resource**.  
> If you hold one open while waiting for an HTTP call, you're effectively reducing your capacity from 100 concurrent requests to 99, often by a lot more when combined with other constraints.  
> This is architecture, not a coding preference.

        await _outbox.PublishAsync(new OrderConfirmationSentEvent { ... }, ct);
    }
}

// ChargePayment — HTTP handler: no DB, only external call
public class ChargePayment : INotificationHandler<OrderPlacedEvent>
{
    public async Task Handle(OrderPlacedEvent evt, CancellationToken ct)
    {
        // HTTP only — no DB connection held
        var payment = await _httpClient.PostAsJsonAsync("/payments", evt.Amount, ct);

        if (payment.IsFaulted) await _outbox.PublishAsync(new PaymentFailedEvent { ... }, ct);
    }
}
```

### Connection pool sizing guidance

| Scenario | Suggested pool size | Notes |
|----------|--------------------|-------|
| Low-traffic (<10 req/s) | 10 | Default sufficient |
| Medium-traffic (10-100 req/s) | 50-100 | Keep connections short |
| High-traffic (>100 req/s) | 200+ | Use async, avoid blocking |
| HTTP-heavy handlers | Increase by 50% | Each HTTP handler may reserve a connection |

### Table logging for write handlers

Write handlers that make changes should **log every change** using the outbox pattern:

```sql
CREATE TABLE OutboxEvents (
    Id BIGINT IDENTITY PRIMARY KEY,
    ActorType VARCHAR(50),
    ActorId UNIQUEIDENTIFIER,
    Action VARCHAR(50),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
```

This provides:
- An audit trail of every change
- The ability to replay or reconcile state
- Visibility into what the saga did


    // 4. Call external HTTP API (connection pool exhausted!)
    var payment = await _httpClient.PostAsJsonAsync("/payments", new { amount = total });
}
```

### Consequences

- **Connection pool exhaustion** — if HTTP call takes 500ms and 10 concurrent requests each hold a connection, you exhaust the pool (typical pool size is 100)
- **Long-running transactions** — locks held for longer than needed; higher risk of deadlocks
- **Hard to observe** — the transaction duration appears in metrics mixed with application latency
- **Difficult to retry** — partial state changes require compensating transactions

- Related: ADR-0026 (outbox pattern), ADR-0027 (observability), ADR-0029 (database migrations)