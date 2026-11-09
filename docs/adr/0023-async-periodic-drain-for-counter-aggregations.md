# ADR-0023: Async periodic drain for counter aggregations

- Status: Proposed
- Date: 2026-03-30
- By: Usman Rafiq
- Related: ADR-0022 (Redis write-behind cache), ADR-0002 (repository structure)

## Context

When usage counters are cached in Redis, they must eventually be **drained** into the SQL database.

### The naive approach

Draining on every write is exactly what we are trying to avoid: the flush becomes a distributed write storm, and the same SQL contention returns.

### The periodic approach

A background process aggregates and flushes batches at a fixed interval:

- **Startup**: schedule a periodic timer
- **Interval**: e.g., every 30 s (configurable per counter type)
- **Flush**: read all pending counters from Redis, combine into a batch, write to SQL
- **Cleanup**: remove drained Redis keys to prevent memory growth

### Requirements

- The drain must be **idempotent** (resumable after crash; no double-counted rows)
- The drain must have **backpressure** — if SQL cannot keep up, slow down or throttle
- The drain job must be **observable** (logs, metrics, alerts)
- Multiple instances of the same service must **not** double-drain the same keys

## Decision

We will implement an **async periodic drain** using .NET's `PeriodicTimer` and a **distributed lock** (or single-instance marker) to ensure only one process drains at a time.

### Telemetry instrumentation

The drain job emits:
- `CounterDrainBatchSent` — count of rows sent, seconds since last flush
- `CounterDrainSuccess` / `CounterDrainFailure` — outcome counters
- `RedisKeyDuration` — how long keys have been waiting (latency), when drained

### Drain job lifecycle

```
1. Check distributed lock (redis SETNX or DB marker)
2. If lock acquired:
   a. Read all pending keys from Redis (SCAN, not KEYS for safety)
   b. Batch convert to SQL INSERT rows
   c. Execute bulk insert
   d. Delete drained keys (DEL)
   e. Release lock
3. Sleep until next interval
```

### Implementation sketch

```csharp
public class CounterDrainJob : BackgroundService
{
    private readonly PeriodicTimer _timer;
    private readonly IRedisCounterStore _redis;
    private readonly ISqlCounterStore _sql;
    private readonly ILogger<CounterDrainJob> _logger;

    public override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await DrainAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Counter drain job failed");
            }
        }
    }

    private async Task DrainAsync(CancellationToken ct)
    {
        // 1. Distributed lock to prevent double-drain
        if (!await _lock.acquireAsync("drain-lock", ct)) return;

        try
        {
            // 2. Scan and drain
            var batches = await _redis.ScanAndDrainAsync(ct);

            // 3. Bulk insert
            await _sql.BulkInsertAsync(batches, ct);

            // 4. Cleanup
            await _redis.RemoveDrainedKeysAsync(ct);
        }
        finally
        {
            await _lock.releaseAsync();
        }
    }
}
```

## Alternatives considered

### Option A: On-demand drain (on each flush)

- **Cons**: No guarantee the caller's process will run the drain; multiple instances would race

### Option B: On-demand drain (triggered by HTTP endpoint)

- **Pros**: Simple
- **Cons**: Not automated; requires a running endpoint; no backpressure; fragile in auto-scaling groups

### Option C: Tie drain to process lifetime (process exit flush)

- **Cons**: Loses counts if the process crashes unexpectedly; not ideal for production

### Option D: Use a message queue (e.g., Azure Service Bus, Kafka)

- **Pros**: Excellent for complex async pipelines
- **Cons**: Overkill and high operational cost for simple counter flushing; Redis + periodic timer is simpler and sufficient

## Consequences

### ✅ Good

- Backpressure is naturally handled by the timer interval
- The drain is **decoupled** from user requests
- Works across multiple application instances
- Zero-impact dump into SQL during idle windows

### ❌ Bad

- Transient counters lost on crash between drain cycle and SQL flush (mitigated by interval, but not zero)
- Requires careful lock management to prevent double-drain
- Need to handle restart before the first drain completes (counters must be persisted in Redis before being lost)

### 🔧 Follow-ups

- Implement monitoring dashboard for drain lag
- Add alerts if drain frequency drops below expected
- Configure background service to start on application startup

## References

- ADR-0022: Redis write-behind cache
- samples/redis-writebehind-counter/CounterService/CounterDrainJob.cs

## Architect Note

> The periodic drain pattern is a classic "batch out of a fast queue" pattern.  
> It is a natural fit for counters, KPIs, and analytics events — anything where **aggregate precision > real-time latency** and you can tolerate at most N seconds of lag.
