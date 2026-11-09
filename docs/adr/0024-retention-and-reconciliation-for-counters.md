# ADR-0024: Data retention and reconciliation for counters

- Status: Proposed
- Date: 2026-03-30
- By: Usman Rafiq
- Related: ADR-0022 (Redis write-behind cache), ADR-0023 (async periodic drain), ADR-0002 (repository structure)

## Context

Write-behind caching with periodic drain introduces two operational risks:

1. **Counter drift**: counts in Redis may not match the SQL database due to crashes, restarts, or partial flushes
2. **Unbounded Redis memory**: without pruning, the Redis keys accumulate indefinitely, risking OOM

Both must be explicitly managed.

### The reconciliation problem

After a crash, Redis may have pending counters that were never drained. If the drain
job resumes and blindly writes the same keys again, you get **double counting**.
Conversely, if the drain job succeeds but the Redis `DEL` fails, keys are lost.

This means we need a **verification step** to reconcile Redis vs. SQL counts.

## Decision

We will implement:

### 1. Aggregated row retention

SQL aggregates are partitioned by day and retained with a configurable TTL:

```
Daily aggregates: retained for 90 days
Hourly aggregates: retained for 30 days
```

Entities older than the TTL are purged in a nightly batch job.

### 2. Redis key TTL

Individual counter keys in Redis are assigned a TTL equal to the drain interval
multiplied by a safety factor, so they can be re-scanned after a restart.

For example, with a 30-second drain interval:

```
TTL = 30s × 3 = 90 seconds (minimal safety margin)
```

### 3. Reconciliation job

Every hour, a reconciliation job compares Redis cumulative counts against the
latest SQL aggregates for each counter type:

- **No drift**: normal operation
- **Drift detected**: dump to a "drift log" and investigate
- **Reconciliation mode**: optional "hard sync" that resets SQL to match Redis

### 4. Metric export

The following metrics are exported for monitoring:

- `counter_drain_batch_size` (histogram, per batch)
- `counter_drain_latency_ms` (histogram, sustained flush time)
- `counter_drain_run_count` (counter, incremented each run)
- `counter_drain_failure_total` (counter, incremented on failure)
- `counter_redis_pending_count` (gauge, number of keys pending drain)

## Alternatives considered

### Option A: Full audit trail using an event log

- **Pros**: Perfect correctness; can replay any drift
- **Cons**: Huge storage overhead; high complexity; overkill for counters

### Option B: Manual re-tally on a dump

- **Pros**: Simple
- **Cons**: Requires human intervention; slow; not feasible at scale

### Option C: Versioned counters (monotonic key versions)

- **Pros**: Resolves double-counting deterministically
- **Cons**: Adds significant complexity to the counter schema and API

## Consequences

### ✅ Good

- Redis memory is bounded
- SQL rows are pruned automatically
- Drift is detectable and corrigible
- Operational visibility into the draining pipeline

### ❌ Bad

- The reconciliation job adds **some** additional processing overhead
- Requires **operational discipline** to handle drift alerts
- Network and database failures can still cause temporary drift before reconciliation

### 🔧 Follow-ups

- Set up dashboards for `counter_drain_run_count`, `counter_redis_pending_count`, `counter_drain_batch_size`
- Configure alerts when `counter_drain_failure_total` increments
- Establish a weekly reconciliation review cadence
- Monitor Redis memory usage to ensure the TTL strategy keeps it bounded

## References

- ADR-0022: Redis write-behind cache
- ADR-0023: Async periodic drain
- samples/redis-writebehind-counter/CounterService/CounterDrainJob.cs
