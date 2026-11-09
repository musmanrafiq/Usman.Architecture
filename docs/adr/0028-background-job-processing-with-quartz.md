# ADR-0028: Background job processing for domain events (Quartz.NET)

- Status: Proposed
- Date: 2026-03-31
- By: Usman Rafiq
- Related: ADR-0002 (repository structure), ADR-0011 (module boundaries), ADR-0026 (outbox pattern)

## Context

Modern .NET systems need to run **background jobs** that are not tied to user requests:

- Process outbox events and publish to message brokers
- Send email/SMS notifications
- Generate reports and exports
- Clean up old data (retention policies)
- Process batch imports or exports
- Refresh caches or search indexes

### The problem

The naive approach:

```csharp
// BAD: fire-and-forget thread — no reliability, no observability
Task.Run(() => SendEmailAsync(email));

// Also BAD: tying background work to a request — blocks or drops on sync
app.MapPost("/orders", async (Order order) =>
{
    await _repo.Save(order);
    _ = NotifyCustomer(order); // Don't await — fire and forget?
    return Results.Created(...); // Problem: if process crashes, NotifyCustomer lost
});
```

### Constraints

- Jobs must be **retried on failure** (idempotent execution)
- Jobs must be **scheduled** (cron, interval, or at-specific-time)
- Jobs must be **survivable** across application restarts
- Job processing must be **observable** (logs, metrics, alerting)
- Multiple instances of the same service must **not double-process** jobs

## Decision

We will use **Quartz.NET** as the background job scheduler, integrated with:
- **Outbox pattern** (ADR-0026) for reliable event publication
- **ASP.NET Core DI** (using `ISchedulerFactory` from Quartz)
- **Structured logging** (Serilog) for job execution traces

### Job scheduling model

```csharp
// Example: publish outbox events every 5 seconds
IScheduler scheduler = _schedulerFactory.GetScheduler();
await scheduler.JobFactory = new MyJobFactory(services);

await scheduler.Start();

// Trigger: run every 5 seconds, with 5-minute warm-up
var trigger = TriggerBuilder.Create()
    .WithCron("0/5 * * * * ?")  // every 5 seconds
    .Build();

await scheduler.ScheduleJob(new JobDetailImpl(typeof(OutboxPublisherJob), "outbox-publisher"));
await scheduler.ScheduleJob(trigger);
```

### Job design principles

| Principle | Description |
|-----------|-------------|
| **Idempotency** | Job must be safe to re-run (no side effects on re-execution) |
| **Deduplication** | Track job executions (job ID + run time) to prevent duplicate runs |
| **Timeout** | Jobs have a configurable timeout; they are not killed, but logged and retried |
| **Monitoring** | Job execution count, failure count, and duration are emitted as metrics |
| **Graceful shutdown** | On `IHostApplicationLifetime.ApplicationStopping`, jobs are paused, not killed mid-flight |

### Example job

```csharp
public class OutboxPublisherJob : IJob
{
    private readonly ILogger<OutboxPublisherJob> _logger;
    private readonly IOutboxStore _outbox;

    public Task Execute(IJobExecutionContext context)
    {
        var batches = _outbox.GetPendingBatches(maxRows: 500);

        foreach (var batch in batches)
        {
            var success = _eventBus.Publish(batch.Events);
            if (success)
            {
                _outbox.MarkPublished(batch);
            }
            else
            {
                _outbox.RecordFailure(batch, context.TriggerId);
            }
        }

        _logger.LogInformation("Published {Count} outbox events", batches.Count);
        return Task.CompletedTask;
    }
}
```

## Alternatives considered

### Option A: `BackgroundService` + `PeriodicTimer`

- **Pros**: Simple, built into .NET
- **Cons**: No built-in retry, scheduling, or job persistence
- **Verdict**: Rejected for production (use for trivial fire-and-forget)

### Option B: Hangfire

- **Pros**: Easy UI for monitoring, recurring jobs, job state management
- **Cons**: Vendor-specific; requires Redis as backing store
- **Verdict**: Accepted as alternative to Quartz; Quartz chosen for portability

### Option C: Azure Functions / AWS Lambda

- **Pros**: Serverless; no infrastructure to manage; auto-scaling
- **Cons**: Vendor lock-in; cold starts; not suitable for all workloads
- **Verdict**: Accepted for specific workloads; Quartz for general-purpose background jobs

### Option D: Timer + thread pool (raw `System.Threading.Timer`)

- **Pros**: No dependencies
- **Cons**: No retry, scheduling, or monitoring; no job persistence
- **Verdict**: Rejected (insufficient for production)

## Consequences

### ✅ Good

- **Reliable job execution** — jobs survive application restarts
- **Configurable retry** — exponential backoff, max attempts
- **Observability** — job execution metrics in Prometheus/Grafana
- **Schedule flexibility** — cron, interval, one-off, daily/weekly
- **Cluster-safe** — jobs run on only one instance (job persistence + locking)

### ❌ Bad

- **Additional complexity** — need to manage job definitions, triggers, and jobs
- **Startup latency** — jobs are registered at application startup
- **Job failure** can cascade if not properly isolated
- **Timezone handling** — cron expressions are timezone-sensitive; need explicit config

### 🔧 Follow-ups

- Set up **job monitoring dashboard**: success rate, latency, queue depth
- Configure **alerting** for jobs that fail repeatedly (max attempts exceeded)
- Implement **job rate limiting** to avoid burst behavior
- Add **job replay** capability (re-run a failed job manually)
- Use **distributed lock** for jobs that must run on a single instance

## References

- ADR-0002: How this repository is structured
- ADR-0011: Module boundary rules
- ADR-0026: Outbox pattern for reliable event publishing

## Architect Note

> A background job should be treated as **a first-class operation** with the same rigor as a user request.  
> If it can fail, it will fail. Design for failure from day one.
