# ADR-0022 / ADR-0023 Sample: Redis Write-Behind Counter

This sample demonstrates the **write-behind cache pattern** for usage counters:

- **Increment counters in Redis** (sub-millisecond)
- **Periodically drain batches to SQL** (reducible DB load)
- Cleanly separates the **fast path** (tracking usage) from the **slow path** (persisting aggregates)

---

## What this sample shows

### 1. High-throughput counter recording

```csharp
// Typical usage — called on every API hit, page view, etc.
var newValue = await _counterStore.IncrementAsync(
    tenantId: "tenant_A",
    counterType: "api_token_usage",
    dimensionKey: "dashboard",
    granularity: "hour",
    count: 1);
```

Redis handles this in **microseconds** — the SQL database is **not touched** on every event.

### 2. Periodic drain

A background `PeriodicTimer` runs every 30 seconds (configurable):

1. Acquire a distributed lock (so multiple instances don't double-drain)
2. `SCAN` Redis for pending counters (non-blocking)
3. Bulk-insert rows to SQL (`SaveChangesBatch`)
4. Delete drained keys from Redis

### 3. Multitenant awareness

The same counter store works for different tenants, features, and granularities:

```
counter:{api_token_usage}:{day}:{tenant_A}:{dashboard_page}
counter:{page_views}:{hour}:{tenant_B}:{summary}
counter:{feature_usage}:{day}:{tenant_A}:{export_feature}
```

---

## Project structure

```
samples/redis-writebehind-counter/
├── redis-writebehind-counter.csproj
├── Program.cs                          # Entry point: simulates 20 tracked events
├── CounterService/
│   ├── ICounterStore.cs                # Interface: high-level contract
│   ├── RedisCounterStore.cs            # Redis cache (HINCRBY per key)
│   ├── SqlCounterStore.cs              # SQL bulk insert
│   ├── CounterDrainJob.cs              # Periodic background drain (30s default)
│   └── ISqlCounterStore.cs             # Bulk insert interface
└── Data/
    ├── CounterContext.cs               # EF Core DbContext
    └── CounterAggregated.cs            # Sink entity
```

---

## How to run

**Prerequisites:**

- .NET 9 SDK
- A running Redis server (Docker or local installation)

**Run:**

```bash
# Start Redis (Docker)
docker run -d -p 6379:6379 redis:7-alpine

# Build and run
dotnet run --project samples/redis-writebehind-counter
```

---

## Expected output

```
=== Redis Write-Behind Counter Demo ===

[Event 1/20] api_token_usage — day: tenant_A → Redis now at 1
[Event 5/20] api_token_usage — hour: tenant_B → Redis now at 1
...
All 20 counter events have been recorded into Redis (fast path).

Drain job is running in the background every 30 seconds (configurable).
Press Ctrl+C to exit.
```

---

## What is intentionally omitted

- Real SQL Server connection (SQLite used for demo)
- Distributed lock implementation (uses `SemaphoreSlim` here; production uses DB marker or Redis SETNX)
- Dead-letter / reconciliation queue
- Dashboard / metrics export
- Configurable flush intervals per counter type

---

## Related documents

- **ADR-0022**: Redis write-behind cache for usage counters
- **ADR-0023**: Async periodic drain for counter aggregations
- **ADR-0011**: Module boundary rules (this module communicates only through interfaces)
