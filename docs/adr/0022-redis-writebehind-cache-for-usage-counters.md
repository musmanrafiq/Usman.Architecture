# ADR-0022: Redis write-behind cache for usage counters

- Status: Proposed
- Date: 2026-03-30
- By: Usman Rafiq
- Related: ADR-0002 (repository structure), ADR-0011 (module boundaries), ADR-0023 (async periodic drain)

## Context

Modern systems need to track many kinds of **"usage counters"** at high frequency:

- API token consumption per tenant/user/app
- Page views, feature usage events, workflow steps
- Notification counts, invitation counts, rate-limit buckets

### The naive approach

```csharp
// BAD: hits the database on EVERY single event
public async Task RecordTokenUsageAsync(string tenantId, int count = 1)
{
    var entity = await _dbContext.TenantTokenUsage
        .FirstOrDefaultAsync(x => x.TenantId == tenantId);
    entity.Count += count;
    await _dbContext.SaveChangesAsync(); // ← N+1 writes!
}
```

### The problems

- A single dashboard or high-traffic endpoint can generate **thousands of writes per second**
- Each write incurs:
  - TCP round-trip to PostgreSQL/MySQL
  - SQL statement parsing, plan generation, execution
  - Transaction overhead
  - Connection pool pressure
- This quickly saturates the database, undermining the very analytics being tracked
- The cost of tracking grows linearly with traffic, which is a **monolithic coupling** of operational cost to business growth

### Constraints

- Counters must survive crashes or restarts
- Draining to SQL must be **interval-based** (not on every event)
- The system must tolerate Redis or SQL outages
- The counter aggregation logic must remain **simple and testable**
- Other modules must not need to know the internal flush mechanism

## Decision

We will use **Redis as a write-behind cache** for usage counters.

### Architecture

```
┌─────────────┐     Increment     ┌──────────────┐     Flush     ┌──────────────┐
│ Application │ ────────────────► │   Redis      │ ────────►     │    SQL DB    │
│  (counter)  │                   │ Write cache  │  (timer/job)  │ Aggregate    │
└─────────────┘                   └──────────────┘               └──────────────┘
```

1. **Increment in Redis** on every usage event (sub-millisecond)
2. **Batch flush** to SQL DB at a configured interval (e.g., every 30–60 s) using a background job
3. **Cumulative counters** are stored in SQL as aggregated rows
4. **Reconciliation** periodically verifies Redis vs. SQL counts to ensure no loss

### Core components

| Component | Responsibility |
|-----------|-----------------|
| `ICounterStore` | Caches `StringIncrement` / `HashIncrement` in Redis |
| `ICounterDrainJob` | Runs on a `PeriodicTimer` and bulk-inserts performed rows into SQL |
| `CounterConfig` | Allows per-counter configuration of flush interval and aggregate key |
| `MetricRecorder` | Higher-level facade that modules call to track "user did X" |

### Data model

```sql
-- Redis: per-tenant hash per counter type (e.g., hset:api_token_usage:{tenantId})
-- Redis: cumulative "drain" bucket e.g. hset:counter_bucket:{date}:{type}

CREATE TABLE CounterAggregated (
    Id BIGINT IDENTITY PRIMARY KEY,
    CounterType VARCHAR(50) NOT NULL,   -- e.g., 'api_token_usage', 'page_views'
    Granularity VARCHAR(20) NOT NULL,   -- 'day', 'hour', 'minute'
    DimensionKey VARCHAR(200) NOT NULL, -- e.g., tenant id, user id
    Value BIGINT NOT NULL DEFAULT 0,
    RecordedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Counter UNIQUE (CounterType, Granularity, DimensionKey, RecordedAt)
);
```

## Alternatives considered

### Option A: CQRS + Event sourcing (event store only)

- **Pros**: Eventual replay, perfect audit trail, no aggregation bugs
- **Cons**: Significant transition cost; requires event schema redesign; many event types; overkill for simple counters

### Option B: In-memory dictionary + periodic batch (no Redis)

- **Pros**: No new dependency; simple
- **Cons**: **Loses counts on process restart**; does not distribute across multiple application instances; not suitable for multi-host deployments

### Option C: Direct DB writes (no caching)

- **Pros**: Simple to implement
- **Cons**: DB becomes a bottleneck; writes scale with all traffic; no resilience; high tail latency risk

### Option D: Redis in-memory with periodic full-sync

- **Pros**: Best performance; robust
- **Cons**: Requires additional machinery for reconciliation; higher complexity than write-behind approach

## Consequences

### ✅ Good

- Counter updates become **sub-millisecond** and largely independent of SQL DB load
- SQL DB experiences **burstless, aggregated writes** — predictable, batched, low frequency
- Supports **multi-instance deployments**: all instances share the same Redis backend
- Counters are **survivable** across application restarts
- Easy to add metrics tracking, dashboards, rate limiting, and quotas without DB contention

### ❌ Bad

- Slight complexity in backing up and reconciling Redis vs. SQL
- Occasional **lag** between the usage event and SQL persistence (depending on interval)
- Requires managing an additional Redis instance
- Requires periodic reconciliation to prevent Redis memory growth

### 🔧 Follow-ups

- Implement a reconciliation job that compares Redis cumulative counts against SQL for drift
- Add monitoring on Redis memory usage; cap per-key TTL
- Add telemetry over flush frequency, batch size, and any dropped items
- Define retention policy: how long aggregated rows live in SQL

## References

- ADR-0011: Module boundary rules
- ADR-0023: Async periodic drain
- samples/redis-writebehind-counter/

## Architect Note

> Write-behind caching is **not free** — it trades consistency for performance.  
> For counters that are analytics-only (no payment, no workflow approval), this trade-off is usually worth it.  
> If the counter **must** be consistent with the event in real time, use direct writes or exactly-once delivery, not write-behind.
